using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FlightJournal.Web.Logging;
using Microsoft.ApplicationInsights;
using Skyhop.FlightAnalysis.Models;

namespace FlightJournal.Web.Aprs
{
    /// <summary>
    /// The only owner of the flight-analysis engine. Any number of listener threads may <see cref="Post"/>;
    /// a single consumer thread dedupes, feeds the engine, expires idle aircraft and recycles the engine
    /// nightly or when its spatial index faults. Telemetry is aggregated and never carries aircraft identifiers.
    /// </summary>
    public sealed class FlightAnalysisWorker : IDisposable
    {
        private static readonly TimeSpan ExpiryScanInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan MetricInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan FaultWindow = TimeSpan.FromHours(1);
        private const int TakeTimeoutMilliseconds = 1000;

        private readonly Func<IFlightAnalysisEngine> _engineFactory;
        private readonly TelemetryClient _telemetry;
        private readonly Func<DateTime> _utcNow;
        private readonly FlightAnalysisWorkerOptions _options;
        private readonly BlockingCollection<PositionUpdate> _queue;
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private readonly object _failureLock = new object();
        private Thread _consumer;

        // Owned by the consumer thread
        private readonly Dictionary<string, TrackedAircraft> _tracked = new Dictionary<string, TrackedAircraft>();
        private readonly Queue<DateTime> _faultRecycles = new Queue<DateTime>();
        private IFlightAnalysisEngine _engine;
        private DateTime _lastExpiryScan;
        private DateTime _lastMetricFlush;
        private DateTime _lastNightlyRecycleDate;
        private DateTime _suspendedUntil;
        private long _droppedReported;

        // Guarded by _failureLock (engine events may be raised from library threads)
        private DateTime _lastExceptionSample = DateTime.MinValue;
        private int _suppressedExceptions;
        private int _failuresSinceMetric;

        private long _accepted;
        private long _completed;
        private long _dropped;
        private long _processed;
        private long _failures;
        private int _recycles;
        private volatile bool _suspended;

        public event EventHandler<OnTakeoffEventArgs> OnTakeoff;
        public event EventHandler<OnLandingEventArgs> OnLanding;
        public event EventHandler<OnRadarContactEventArgs> OnRadarContact;
        public event EventHandler<OnContextDisposedEventArgs> OnContextDispose;

        public FlightAnalysisWorker(Func<IFlightAnalysisEngine> engineFactory, TelemetryClient tc, Func<DateTime> utcNow, FlightAnalysisWorkerOptions options)
        {
            _engineFactory = engineFactory;
            _telemetry = tc;
            _utcNow = utcNow;
            _options = options;
            _queue = new BlockingCollection<PositionUpdate>(options.QueueCapacity);
        }

        public long ProcessedCount => Interlocked.Read(ref _processed);
        public long DroppedCount => Interlocked.Read(ref _dropped);
        public long FailureCount => Interlocked.Read(ref _failures);
        public int RecycleCount => Volatile.Read(ref _recycles);
        public bool IsSuspended => _suspended;

        public void Start()
        {
            var now = _utcNow();
            _lastExpiryScan = now;
            _lastMetricFlush = now;
            // Starting inside the recycle hour counts as today's recycle
            _lastNightlyRecycleDate = now.Hour == _options.RecycleHourUtc ? now.Date : DateTime.MinValue;
            _engine = CreateEngine();

            _consumer = new Thread(Run) { IsBackground = true, Name = nameof(FlightAnalysisWorker) };
            _consumer.Start();
        }

        /// <summary>Non-blocking; returns false (and counts the drop) when the queue is full or stopped.</summary>
        public bool Post(PositionUpdate update)
        {
            if (update == null)
                return false;

            Interlocked.Increment(ref _accepted);
            bool added;
            try
            {
                added = _queue.TryAdd(update);
            }
            catch (InvalidOperationException)
            {
                // Adding completed: the worker is shutting down
                added = false;
            }

            if (!added)
            {
                Interlocked.Decrement(ref _accepted);
                Interlocked.Increment(ref _dropped);
            }
            return added;
        }

        /// <summary>Waits until every accepted update has been processed, including the maintenance that follows it.</summary>
        public bool WaitForIdle(TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < timeout)
            {
                if (_queue.Count == 0 && Interlocked.Read(ref _completed) >= Interlocked.Read(ref _accepted))
                    return true;
                Thread.Sleep(1);
            }
            return false;
        }

        private void Run()
        {
            var token = _stop.Token;
            while (!token.IsCancellationRequested)
            {
                PositionUpdate update;
                try
                {
                    _queue.TryTake(out update, TakeTimeoutMilliseconds, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // The consumer must never die: a failure here would silently stop all flight analysis
                try
                {
                    if (update != null)
                        Process(update);
                    RunMaintenance();
                }
                catch (Exception ex)
                {
                    RecordFailure(ex);
                }

                if (update != null)
                    Interlocked.Increment(ref _completed);
            }
        }

        private void Process(PositionUpdate update)
        {
            var now = _utcNow();
            if (_suspended)
            {
                if (now < _suspendedUntil)
                {
                    Interlocked.Increment(ref _dropped);
                    return;
                }
                Resume();
            }

            if (!_tracked.TryGetValue(update.Aircraft, out var aircraft))
            {
                aircraft = new TrackedAircraft();
                _tracked.Add(update.Aircraft, aircraft);
            }
            aircraft.LastSeenUtc = now;
            if (!aircraft.TryAddTimestamp(update.TimeStamp))
                return; // duplicate from an overlapping listener area

            if (CallEngine(e => e.Enqueue(update)))
                Interlocked.Increment(ref _processed);
        }

        /// <summary>Runs an engine call; an index fault recycles the engine, any other failure is counted.</summary>
        private bool CallEngine(Action<IFlightAnalysisEngine> call)
        {
            try
            {
                call(_engine);
                return true;
            }
            catch (Exception ex) when (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
            {
                RecordFailure(ex);
                OnIndexFault();
                return false;
            }
            catch (Exception ex)
            {
                RecordFailure(ex);
                return false;
            }
        }

        private void RunMaintenance()
        {
            var now = _utcNow();

            if (now - _lastExpiryScan >= ExpiryScanInterval)
            {
                _lastExpiryScan = now;
                var idle = _tracked
                    .Where(t => now - t.Value.LastSeenUtc > _options.ContextIdleExpiry)
                    .Select(t => t.Key)
                    .ToList();
                foreach (var aircraft in idle)
                {
                    _tracked.Remove(aircraft);
                    if (!CallEngine(e => e.Detach(aircraft)))
                        break; // the engine was recycled or failed; stop using it for this scan
                }
            }

            if (now.Hour == _options.RecycleHourUtc && _lastNightlyRecycleDate != now.Date)
            {
                _lastNightlyRecycleDate = now.Date;
                Recycle("nightly");
            }

            if (now - _lastMetricFlush >= MetricInterval)
            {
                _lastMetricFlush = now;
                int failures;
                lock (_failureLock)
                {
                    failures = _failuresSinceMetric;
                    _failuresSinceMetric = 0;
                }
                var dropped = Interlocked.Read(ref _dropped);
                _telemetry.TrackMetric("FlightAnalysisFailures", failures);
                _telemetry.TrackMetric("FlightAnalysisDropped", dropped - _droppedReported);
                _droppedReported = dropped;
            }
        }

        private void OnIndexFault()
        {
            var now = _utcNow();
            while (_faultRecycles.Count > 0 && now - _faultRecycles.Peek() >= FaultWindow)
                _faultRecycles.Dequeue();

            if (_faultRecycles.Count >= _options.MaxFaultRecyclesPerHour)
            {
                Suspend(now);
                return;
            }

            _faultRecycles.Enqueue(now);
            Recycle("fault");
        }

        private void Suspend(DateTime now)
        {
            _suspended = true;
            _suspendedUntil = now + _options.SuspendDuration;
            Log.Warning($"{nameof(FlightAnalysisWorker)}: repeated index faults, flight analysis suspended for {_options.SuspendDuration}");
            _telemetry.TrackEvent("FlightAnalysisSuspended");
        }

        private void Resume()
        {
            _suspended = false;
            Recycle("resume");
        }

        /// <summary>Replaces the engine. Tracked aircraft are detached from the old engine first, so nothing it still references keeps them alive.</summary>
        private void Recycle(string reason)
        {
            var old = _engine;
            Unsubscribe(old);

            var detachFailures = 0;
            foreach (var aircraft in _tracked.Keys)
            {
                try
                {
                    old.Detach(aircraft);
                }
                catch (Exception)
                {
                    // The old engine's index may be the corrupt one; it is discarded either way. Counted below.
                    detachFailures++;
                }
            }
            _tracked.Clear();

            _engine = CreateEngine();
            Interlocked.Increment(ref _recycles);
            Log.Information($"{nameof(FlightAnalysisWorker)}: flight analysis engine recycled ({reason})");
            _telemetry.TrackEvent("FlightAnalysisRecycled", new Dictionary<string, string>
            {
                { "Reason", reason },
                { "DetachFailures", detachFailures.ToString() }
            });
        }

        private IFlightAnalysisEngine CreateEngine()
        {
            var engine = _engineFactory();
            engine.OnTakeoff += EngineOnTakeoff;
            engine.OnLanding += EngineOnLanding;
            engine.OnRadarContact += EngineOnRadarContact;
            engine.OnContextDispose += EngineOnContextDispose;
            return engine;
        }

        private void Unsubscribe(IFlightAnalysisEngine engine)
        {
            engine.OnTakeoff -= EngineOnTakeoff;
            engine.OnLanding -= EngineOnLanding;
            engine.OnRadarContact -= EngineOnRadarContact;
            engine.OnContextDispose -= EngineOnContextDispose;
        }

        private void EngineOnTakeoff(object sender, OnTakeoffEventArgs e) => Raise(OnTakeoff, e);
        private void EngineOnLanding(object sender, OnLandingEventArgs e) => Raise(OnLanding, e);
        private void EngineOnRadarContact(object sender, OnRadarContactEventArgs e) => Raise(OnRadarContact, e);
        private void EngineOnContextDispose(object sender, OnContextDisposedEventArgs e) => Raise(OnContextDispose, e);

        /// <summary>A failing subscriber is counted, and never mistaken for an engine index fault.</summary>
        private void Raise<T>(EventHandler<T> handler, T args)
        {
            try
            {
                handler?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                RecordFailure(ex);
            }
        }

        private void RecordFailure(Exception ex)
        {
            Interlocked.Increment(ref _failures);
            var now = _utcNow();
            lock (_failureLock)
            {
                _failuresSinceMetric++;
                if (now - _lastExceptionSample < _options.ExceptionSampleInterval)
                {
                    _suppressedExceptions++;
                    return;
                }

                _telemetry.TrackException(ex, new Dictionary<string, string>
                {
                    { "Source", nameof(FlightAnalysisWorker) },
                    { "SuppressedSinceLastSample", _suppressedExceptions.ToString() }
                });
                _suppressedExceptions = 0;
                _lastExceptionSample = now;
            }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _stop.Cancel();
            _consumer?.Join(TimeSpan.FromSeconds(5));
            if (_engine != null)
                Unsubscribe(_engine);
        }

        private sealed class TrackedAircraft
        {
            private const int RecentTimestampCount = 16;
            private readonly Queue<DateTime> _order = new Queue<DateTime>();
            private readonly HashSet<DateTime> _recent = new HashSet<DateTime>();

            public DateTime LastSeenUtc { get; set; }

            /// <summary>False when this timestamp was among the recent ones already seen.</summary>
            public bool TryAddTimestamp(DateTime timestamp)
            {
                if (!_recent.Add(timestamp))
                    return false;
                _order.Enqueue(timestamp);
                if (_order.Count > RecentTimestampCount)
                    _recent.Remove(_order.Dequeue());
                return true;
            }
        }
    }
}
