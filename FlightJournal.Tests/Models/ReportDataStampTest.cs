using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlightJournal.Web.Models.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    /// <summary>
    /// The report data stamp and the AvailableDates snapshot cache, shared safely between concurrent requests. Pure: no database.
    /// The stamp is process-wide, so tests compare deltas, and use years no other test touches.
    /// </summary>
    [TestClass]
    public class ReportDataStampTest
    {
        [TestCleanup]
        public void Cleanup()
        {
            AvailableDatesCache.ResetForTests();
        }

        private static string UniqueKey() => "test-" + Guid.NewGuid().ToString("N");

        [TestMethod]
        public void Touch_IncrementsAndRecordsEachDate()
        {
            var d1 = new DateTime(1950, 1, 5);
            var d2 = new DateTime(1950, 1, 6);
            var before = ReportDataStamp.Current;

            ReportDataStamp.Touch(new[] { d1, d2, d1 });

            Assert.AreEqual(before + 1, ReportDataStamp.Current, "One increment per Touch, however many dates");
            Assert.AreEqual(ReportDataStamp.Current, ReportDataStamp.LastChange(ReportPeriod.ForDay(d1)));
            Assert.AreEqual(ReportDataStamp.Current, ReportDataStamp.LastChange(ReportPeriod.ForDay(d2)));
        }

        [TestMethod]
        public void Touch_NoDates_DoesNotIncrement()
        {
            var before = ReportDataStamp.Current;

            ReportDataStamp.Touch(Enumerable.Empty<DateTime>());

            Assert.AreEqual(before, ReportDataStamp.Current);
        }

        [TestMethod]
        public void TouchAll_IncrementsWithoutDates()
        {
            var before = ReportDataStamp.Current;

            ReportDataStamp.TouchAll();

            Assert.AreEqual(before + 1, ReportDataStamp.Current);
            Assert.AreEqual(0, ReportDataStamp.LastChange(ReportPeriod.ForDay(new DateTime(1950, 12, 31))), "No date is recorded");
        }

        [TestMethod]
        public void LastChange_IsTheMaxWithinThePeriod()
        {
            ReportDataStamp.Touch(new[] { new DateTime(1951, 3, 10) });
            var s1 = ReportDataStamp.Current;
            ReportDataStamp.Touch(new[] { new DateTime(1951, 3, 31) });
            var s2 = ReportDataStamp.Current;
            ReportDataStamp.Touch(new[] { new DateTime(1951, 4, 1) });
            var s3 = ReportDataStamp.Current;

            Assert.AreEqual(s1, ReportDataStamp.LastChange(ReportPeriod.ForDay(new DateTime(1951, 3, 10))));
            Assert.AreEqual(s2, ReportDataStamp.LastChange(ReportPeriod.ForDay(new DateTime(1951, 3, 31))), "Last day of the month");
            Assert.AreEqual(s2, ReportDataStamp.LastChange(ReportPeriod.ForMonth(1951, 3)), "April 1st is outside March");
            Assert.AreEqual(s3, ReportDataStamp.LastChange(ReportPeriod.ForMonth(1951, 4)));
            Assert.AreEqual(s3, ReportDataStamp.LastChange(ReportPeriod.ForYear(1951)));
            Assert.AreEqual(0, ReportDataStamp.LastChange(ReportPeriod.ForDay(new DateTime(1951, 2, 28))));
            Assert.AreEqual(0, ReportDataStamp.LastChange(ReportPeriod.ForMonth(1951, 5)));
            Assert.AreEqual(0, ReportDataStamp.LastChange(ReportPeriod.ForYear(1952)));
        }

        [TestMethod]
        public void Cache_ConcurrentRequests_SingleBuild_NoExceptions()
        {
            // Guards against the concurrent-request race: no shared Dictionary is mutated, and the herd waits for one rebuild
            var key = UniqueKey();
            var builds = 0;
            var exceptions = new ConcurrentQueue<Exception>();
            var results = new ConcurrentQueue<IReadOnlyDictionary<DateTime, int>>();
            var start = new ManualResetEventSlim(false);
            Func<IDictionary<DateTime, int>> build = () =>
            {
                Interlocked.Increment(ref builds);
                Thread.Sleep(50);
                return new Dictionary<DateTime, int> { { new DateTime(2001, 1, 1), 3 } };
            };

            var threads = Enumerable.Range(0, 16).Select(_ => Task.Factory.StartNew(() =>
            {
                start.Wait();
                try
                {
                    results.Enqueue(AvailableDatesCache.GetOrBuild(key, build, DateTime.UtcNow));
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }, TaskCreationOptions.LongRunning)).ToArray();
            start.Set();
            Task.WaitAll(threads);

            Assert.AreEqual(0, exceptions.Count, string.Join("\n", exceptions));
            Assert.AreEqual(1, builds, "A single rebuild for the whole herd");
            Assert.AreEqual(16, results.Count);
            var first = results.First();
            Assert.IsTrue(results.All(r => ReferenceEquals(r, first)), "Every caller gets the same snapshot");
        }

        [TestMethod]
        public void Cache_Snapshot_IsReadOnly()
        {
            var source = new Dictionary<DateTime, int> { { new DateTime(2001, 1, 1), 3 } };

            var snapshot = AvailableDatesCache.GetOrBuild(UniqueKey(), () => source, DateTime.UtcNow);
            source[new DateTime(2001, 1, 2)] = 4;

            Assert.AreEqual(1, snapshot.Count, "A private copy: later changes to the builder's dictionary don't leak in");
            var asDictionary = snapshot as IDictionary<DateTime, int>;
            Assert.IsNotNull(asDictionary);
            Assert.IsTrue(asDictionary.IsReadOnly);
            try
            {
                asDictionary.Add(new DateTime(2001, 1, 3), 5);
                Assert.Fail("The snapshot must not be mutable");
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        public void Cache_OlderThanMaxAge_Rebuilds()
        {
            var key = UniqueKey();
            var builds = 0;
            Func<IDictionary<DateTime, int>> build = () =>
            {
                builds++;
                return new Dictionary<DateTime, int>();
            };
            var t0 = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            AvailableDatesCache.GetOrBuild(key, build, t0);
            AvailableDatesCache.GetOrBuild(key, build, t0.AddHours(23));
            Assert.AreEqual(1, builds, "Still fresh within MaxAge");

            AvailableDatesCache.GetOrBuild(key, build, t0 + AvailableDatesCache.MaxAge);
            Assert.AreEqual(2, builds, "Rebuilt once MaxAge has passed");
        }

        [TestMethod]
        public void Cache_StampChange_RebuildsOnce_ThenWarm()
        {
            var key = UniqueKey();
            var builds = 0;
            Func<IDictionary<DateTime, int>> build = () =>
            {
                builds++;
                return new Dictionary<DateTime, int>();
            };
            var now = DateTime.UtcNow;

            var cold = AvailableDatesCache.GetOrBuild(key, build, now);
            var warm = AvailableDatesCache.GetOrBuild(key, build, now);
            Assert.AreEqual(1, builds);
            Assert.AreSame(cold, warm);

            ReportDataStamp.TouchAll();
            var rebuilt = AvailableDatesCache.GetOrBuild(key, build, now);
            var warmAgain = AvailableDatesCache.GetOrBuild(key, build, now);

            Assert.AreEqual(2, builds, "One rebuild after the stamp changed");
            Assert.AreNotSame(cold, rebuilt);
            Assert.AreSame(rebuilt, warmAgain);
        }
    }
}
