using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    /// <summary>
    /// Load test against the real flight-analysis library. Excluded from the default run:
    /// vstest.console ... /TestCaseFilter:"TestCategory!=Manual"
    /// </summary>
    [TestClass]
    public class FlightAnalysisRaceReproTest
    {
        private const int ThreadCount = 8;
        private const int PostsPerThread = 20000;
        private const int AircraftCount = 64;

        // Synthetic aircraft ids and generated positions only; never real tracker data
        private static PositionUpdate SyntheticUpdate(Random random, int sequence)
        {
            var aircraft = $"TEST{random.Next(AircraftCount):00}";
            return new PositionUpdate(
                aircraft,
                new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(sequence),
                random.NextDouble(),
                random.NextDouble(),
                random.Next(0, 2000),
                random.Next(0, 120),
                random.Next(0, 360));
        }

        /// <summary>Runs the load and returns the exception counts per exception type.</summary>
        private static ConcurrentDictionary<string, int> RunConcurrentLoad(Action<PositionUpdate> post)
        {
            var failures = new ConcurrentDictionary<string, int>();
            var start = new ManualResetEventSlim(false);
            var threads = Enumerable.Range(0, ThreadCount).Select(t => Task.Factory.StartNew(() =>
            {
                var random = new Random(t);
                start.Wait();
                for (var i = 0; i < PostsPerThread; i++)
                {
                    try
                    {
                        post(SyntheticUpdate(random, i));
                    }
                    catch (Exception ex)
                    {
                        failures.AddOrUpdate(ex.GetType().Name, 1, (_, n) => n + 1);
                    }
                }
            }, TaskCreationOptions.LongRunning)).ToArray();
            start.Set();
            Task.WaitAll(threads);
            return failures;
        }

        private static string Describe(ConcurrentDictionary<string, int> failures)
        {
            return failures.IsEmpty ? "none" : string.Join(", ", failures.Select(f => $"{f.Key}={f.Value}"));
        }

        [TestMethod]
        [TestCategory("Manual")]
        public void RealFactory_ConcurrentEnqueue_CorruptsIndex()
        {
            var factory = new FlightContextFactory();

            var failures = RunConcurrentLoad(factory.Enqueue);

            Console.WriteLine($"Direct concurrent Enqueue failures: {Describe(failures)}");
            var indexFaults = failures.Where(f => f.Key == nameof(IndexOutOfRangeException) || f.Key == nameof(ArgumentOutOfRangeException)).Sum(f => f.Value);
            if (indexFaults == 0)
                Assert.Inconclusive("The race did not trigger in this run; rerun or raise the load");
        }

        /// <summary>
        /// The worker removes the concurrent writers to the spatial index, so the quadtree never faults.
        /// The library still has an internal read race of its own (an occasional ArgumentOutOfRangeException
        /// from Enumerable.LastOrDefault inside FlightContextFactory.Enqueue, also seen single-threaded);
        /// the worker counts and recycles on those, so they are reported but not asserted on.
        /// </summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void ThroughWorker_SameLoad_NoQuadtreeFaults()
        {
            var channel = new CapturingChannel();
            var telemetry = new TelemetryClient(new TelemetryConfiguration
            {
                TelemetryChannel = channel,
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
            });
            // Sample every exception so each failure's origin can be checked
            var options = new FlightAnalysisWorkerOptions { QueueCapacity = ThreadCount * PostsPerThread, ExceptionSampleInterval = TimeSpan.Zero };
            using (var worker = new FlightAnalysisWorker(() => new SkyhopFlightAnalysisEngine(), telemetry, () => DateTime.UtcNow, options))
            {
                worker.Start();

                var postFailures = RunConcurrentLoad(u => worker.Post(u));

                Assert.IsTrue(worker.WaitForIdle(TimeSpan.FromMinutes(5)), "Worker did not drain in time");
                var exceptions = channel.Items.OfType<ExceptionTelemetry>().Select(e => e.Exception.ToString()).ToList();
                var quadtreeFaults = exceptions.Count(e => e.Contains("NetTopologySuite.Index.Quadtree"));
                Console.WriteLine($"Through worker: processed={worker.ProcessedCount} dropped={worker.DroppedCount} failures={worker.FailureCount} recycles={worker.RecycleCount} quadtreeFaults={quadtreeFaults}");
                Assert.AreEqual("none", Describe(postFailures), "Post must never throw");
                Assert.AreEqual(0, quadtreeFaults, "The spatial index must never fault once the worker is its only caller");
            }
        }

        private sealed class CapturingChannel : ITelemetryChannel
        {
            private readonly ConcurrentQueue<ITelemetry> _items = new ConcurrentQueue<ITelemetry>();

            public IEnumerable<ITelemetry> Items => _items.ToArray();
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
