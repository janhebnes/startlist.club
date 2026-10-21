using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlightJournal.Web.Aprs;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyhop.FlightAnalysis;
using Skyhop.FlightAnalysis.Models;

namespace FlightJournal.Tests.Aprs
{
    [TestClass]
    public class FlightAnalysisWorkerTest
    {
        private static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(30);
        private static readonly DateTime Day1 = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        private readonly List<FakeEngine> _engines = new List<FakeEngine>();
        private readonly TestClock _clock = new TestClock(Day1);
        private readonly InMemoryChannel _channel = new InMemoryChannel();
        private FlightAnalysisWorker _worker;

        // Synthetic aircraft ids and positions only; never real tracker data
        private static PositionUpdate Update(string aircraft, int second)
        {
            return new PositionUpdate(aircraft, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(second), 0.5, 0.5, 100, 50, 90);
        }

        private FlightAnalysisWorker CreateWorker(Func<int, FakeEngine> engineFor = null, FlightAnalysisWorkerOptions options = null)
        {
            var telemetry = new TelemetryClient(new TelemetryConfiguration
            {
                TelemetryChannel = _channel,
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
            });
            _worker = new FlightAnalysisWorker(
                () =>
                {
                    var engine = engineFor?.Invoke(_engines.Count) ?? new FakeEngine();
                    _engines.Add(engine);
                    return engine;
                },
                telemetry,
                () => _clock.Now,
                options ?? new FlightAnalysisWorkerOptions());
            _worker.Start();
            return _worker;
        }

        private void PostAndWait(params PositionUpdate[] updates)
        {
            foreach (var u in updates)
                Assert.IsTrue(_worker.Post(u), "Post was unexpectedly rejected");
            Assert.IsTrue(_worker.WaitForIdle(IdleTimeout), "Worker did not drain in time");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _worker?.Dispose();
        }

        [TestMethod]
        public void Post_FromManyThreads_EngineNeverEnteredConcurrently()
        {
            var worker = CreateWorker(options: new FlightAnalysisWorkerOptions { QueueCapacity = 50000 });

            var posters = Enumerable.Range(0, 8).Select(t => Task.Run(() =>
            {
                for (var i = 0; i < 5000; i++)
                    worker.Post(Update($"TEST{t:00}", i));
            })).ToArray();
            Task.WaitAll(posters);

            Assert.IsTrue(worker.WaitForIdle(IdleTimeout));
            Assert.AreEqual(1, _engines.Count);
            Assert.AreEqual(1, _engines[0].MaxConcurrency, "The engine was entered by more than one thread at once");
            Assert.AreEqual(40000, _engines[0].Enqueued.Count);
            Assert.AreEqual(0, worker.DroppedCount);
        }

        [TestMethod]
        public void Post_DuplicateAircraftAndTimestamp_ProcessedOnce()
        {
            CreateWorker();

            // Overlapping listener areas deliver the same packet twice, sometimes out of order
            PostAndWait(Update("TEST01", 1), Update("TEST01", 2), Update("TEST01", 1), Update("TEST02", 1), Update("TEST01", 2));

            CollectionAssert.AreEqual(
                new[] { "TEST01@1", "TEST01@2", "TEST02@1" },
                _engines[0].Enqueued.Select(u => $"{u.Aircraft}@{u.TimeStamp.Second}").ToArray());
        }

        [TestMethod]
        public void Post_WhenQueueFull_DropsAndCounts_DoesNotBlock()
        {
            var gate = new ManualResetEventSlim(false);
            var entered = new ManualResetEventSlim(false);
            var worker = CreateWorker(_ => new FakeEngine { OnEnqueue = u => { entered.Set(); gate.Wait(); } },
                new FlightAnalysisWorkerOptions { QueueCapacity = 10 });

            Assert.IsTrue(worker.Post(Update("TEST00", 0)));
            Assert.IsTrue(entered.Wait(IdleTimeout), "Consumer never reached the engine");

            var stopwatch = Stopwatch.StartNew();
            var accepted = Enumerable.Range(1, 15).Count(i => worker.Post(Update("TEST01", i)));
            stopwatch.Stop();

            Assert.AreEqual(10, accepted);
            Assert.AreEqual(5, worker.DroppedCount);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, "Post blocked while the queue was full");

            gate.Set();
            Assert.IsTrue(worker.WaitForIdle(IdleTimeout));
        }

        [TestMethod]
        public void IndexFault_RecyclesEngine_AndContinuesProcessing()
        {
            CreateWorker(n => n == 0 ? new FakeEngine { OnEnqueue = u => throw new IndexOutOfRangeException() } : new FakeEngine());

            PostAndWait(Update("TEST01", 1), Update("TEST02", 1));

            Assert.AreEqual(2, _engines.Count, "The engine factory should be called again after an index fault");
            Assert.AreEqual(1, _worker.RecycleCount);
            CollectionAssert.AreEqual(new[] { "TEST02" }, _engines[1].Enqueued.Select(u => u.Aircraft).ToArray());
        }

        [TestMethod]
        public void RepeatedFaults_TripCircuitBreaker_AfterMaxPerHour()
        {
            CreateWorker(_ => new FakeEngine { OnEnqueue = u => throw new ArgumentOutOfRangeException() },
                new FlightAnalysisWorkerOptions { MaxFaultRecyclesPerHour = 6 });

            PostAndWait(Enumerable.Range(0, 10).Select(i => Update($"TEST{i:00}", i)).ToArray());

            Assert.AreEqual(7, _engines.Count, "Initial engine plus at most 6 fault recycles");
            Assert.IsTrue(_worker.IsSuspended);
            Assert.AreEqual(1, _channel.Events("FlightAnalysisSuspended").Count());
        }

        [TestMethod]
        public void IdleAircraft_DetachedAfterExpiry()
        {
            CreateWorker();

            PostAndWait(Update("TEST01", 1));
            _clock.Advance(TimeSpan.FromMinutes(61));
            PostAndWait(Update("TEST02", 2));

            CollectionAssert.AreEqual(new[] { "TEST01" }, _engines[0].Detached.ToArray());
        }

        [TestMethod]
        public void NightlyRecycle_AtConfiguredHour_OncePerDay()
        {
            _clock.Set(new DateTime(2000, 1, 1, 1, 0, 0, DateTimeKind.Utc));
            CreateWorker(options: new FlightAnalysisWorkerOptions { RecycleHourUtc = 2 });

            PostAndWait(Update("TEST01", 1));
            Assert.AreEqual(1, _engines.Count);

            _clock.Set(new DateTime(2000, 1, 1, 2, 5, 0, DateTimeKind.Utc));
            PostAndWait(Update("TEST01", 2));
            Assert.AreEqual(2, _engines.Count);

            _clock.Set(new DateTime(2000, 1, 1, 2, 40, 0, DateTimeKind.Utc));
            PostAndWait(Update("TEST01", 3));
            Assert.AreEqual(2, _engines.Count, "Only one nightly recycle per day");

            _clock.Set(new DateTime(2000, 1, 2, 2, 1, 0, DateTimeKind.Utc));
            PostAndWait(Update("TEST01", 4));
            Assert.AreEqual(3, _engines.Count);
            Assert.AreEqual(2, _channel.Events("FlightAnalysisRecycled").Count(e => e.Properties["Reason"] == "nightly"));
        }

        [TestMethod]
        public void Recycle_DetachesAllTrackedAircraft_OnOldEngine()
        {
            CreateWorker(n => n == 0
                ? new FakeEngine { OnEnqueue = u => { if (u.Aircraft == "TEST04") throw new IndexOutOfRangeException(); } }
                : new FakeEngine());

            PostAndWait(Update("TEST01", 1), Update("TEST02", 1), Update("TEST03", 1), Update("TEST04", 1));

            CollectionAssert.AreEquivalent(new[] { "TEST01", "TEST02", "TEST03", "TEST04" }, _engines[0].Detached.ToArray());
        }

        [TestMethod]
        public void Events_FromRecycledEngine_StillReachSubscribers()
        {
            var worker = CreateWorker(n => n == 0 ? new FakeEngine { OnEnqueue = u => throw new IndexOutOfRangeException() } : new FakeEngine());
            var takeoffs = 0;
            var landings = 0;
            worker.OnTakeoff += (s, e) => Interlocked.Increment(ref takeoffs);
            worker.OnLanding += (s, e) => Interlocked.Increment(ref landings);

            PostAndWait(Update("TEST01", 1));
            Assert.AreEqual(2, _engines.Count);

            _engines[1].RaiseTakeoff();
            _engines[1].RaiseLanding();
            _engines[0].RaiseTakeoff(); // the discarded engine must no longer reach subscribers

            Assert.AreEqual(1, takeoffs);
            Assert.AreEqual(1, landings);
        }

        [TestMethod]
        public void Telemetry_ExceptionsSampled_AtMostOncePerInterval()
        {
            CreateWorker(_ => new FakeEngine { OnEnqueue = u => throw new InvalidOperationException("synthetic failure") },
                new FlightAnalysisWorkerOptions { ExceptionSampleInterval = TimeSpan.FromMinutes(5) });

            PostAndWait(Enumerable.Range(0, 5).Select(i => Update($"TEST{i:00}", i)).ToArray());
            Assert.AreEqual(1, _channel.Items.OfType<ExceptionTelemetry>().Count());
            Assert.AreEqual(5, _worker.FailureCount);
            Assert.AreEqual(0, _worker.RecycleCount, "Non-index failures must not recycle the engine");

            _clock.Advance(TimeSpan.FromMinutes(6));
            PostAndWait(Update("TEST05", 5));
            Assert.AreEqual(2, _channel.Items.OfType<ExceptionTelemetry>().Count());
            Assert.IsTrue(_channel.Items.OfType<MetricTelemetry>().Any(m => m.Name == "FlightAnalysisFailures"));

            var leaked = _channel.Items.OfType<ISupportProperties>()
                .SelectMany(t => t.Properties.Values)
                .Where(v => v != null && v.Contains("TEST"))
                .ToList();
            Assert.AreEqual(0, leaked.Count, "Telemetry must not carry aircraft identifiers: " + string.Join(", ", leaked));
        }

        private sealed class FakeEngine : IFlightAnalysisEngine
        {
            private int _active;
            private int _maxConcurrency;

            public Action<PositionUpdate> OnEnqueue { get; set; }
            public ConcurrentQueue<PositionUpdate> Enqueued { get; } = new ConcurrentQueue<PositionUpdate>();
            public ConcurrentQueue<string> Detached { get; } = new ConcurrentQueue<string>();
            public int MaxConcurrency => _maxConcurrency;

            public event EventHandler<OnTakeoffEventArgs> OnTakeoff;
            public event EventHandler<OnLandingEventArgs> OnLanding;
            public event EventHandler<OnRadarContactEventArgs> OnRadarContact;
            public event EventHandler<OnContextDisposedEventArgs> OnContextDispose;

            public void Enqueue(PositionUpdate update)
            {
                var active = Interlocked.Increment(ref _active);
                try
                {
                    int seen;
                    while (active > (seen = _maxConcurrency) && Interlocked.CompareExchange(ref _maxConcurrency, active, seen) != seen) { }
                    OnEnqueue?.Invoke(update);
                    Enqueued.Enqueue(update);
                }
                finally
                {
                    Interlocked.Decrement(ref _active);
                }
            }

            public void Detach(string aircraft)
            {
                Detached.Enqueue(aircraft);
            }

            public void RaiseTakeoff()
            {
                OnTakeoff?.Invoke(this, new OnTakeoffEventArgs(new Flight()));
            }

            public void RaiseLanding()
            {
                OnLanding?.Invoke(this, new OnLandingEventArgs(new Flight()));
            }
        }

        private sealed class TestClock
        {
            private long _ticks;

            public TestClock(DateTime start)
            {
                Set(start);
            }

            public DateTime Now => new DateTime(Interlocked.Read(ref _ticks), DateTimeKind.Utc);

            public void Set(DateTime utc)
            {
                Interlocked.Exchange(ref _ticks, utc.Ticks);
            }

            public void Advance(TimeSpan by)
            {
                Interlocked.Add(ref _ticks, by.Ticks);
            }
        }

        private sealed class InMemoryChannel : ITelemetryChannel
        {
            private readonly ConcurrentQueue<ITelemetry> _items = new ConcurrentQueue<ITelemetry>();

            public IEnumerable<ITelemetry> Items => _items.ToArray();

            public IEnumerable<EventTelemetry> Events(string name)
            {
                return Items.OfType<EventTelemetry>().Where(e => e.Name == name);
            }

            public bool? DeveloperMode { get; set; }
            public string EndpointAddress { get; set; }

            public void Send(ITelemetry item)
            {
                _items.Enqueue(item);
            }

            public void Flush()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
