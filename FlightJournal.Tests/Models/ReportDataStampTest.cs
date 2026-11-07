using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlightJournal.Web.Models.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    [TestClass]
    public class ReportDataStampTest
    {
        [TestMethod]
        public void LastChange_IsTheMaxStampWithinThePeriod()
        {
            // A year no other test touches
            var inMarch = new DateTime(1971, 3, 10);
            var lateMarch = new DateTime(1971, 3, 31);
            var april = new DateTime(1971, 4, 1);

            Assert.AreEqual(0, ReportDataStamp.LastChange(ReportPeriod.ForYear(1971)), "Nothing touched yet");

            ReportDataStamp.Touch(new[] { inMarch });
            var first = ReportDataStamp.Current;
            ReportDataStamp.Touch(new[] { lateMarch, lateMarch.AddHours(5) });
            var second = ReportDataStamp.Current;
            ReportDataStamp.Touch(new[] { april });
            var third = ReportDataStamp.Current;

            Assert.IsTrue(first < second && second < third);
            Assert.AreEqual(first, ReportDataStamp.LastChange(ReportPeriod.ForDay(inMarch)));
            Assert.AreEqual(second, ReportDataStamp.LastChange(ReportPeriod.ForDay(lateMarch)), "Time of day is ignored");
            Assert.AreEqual(second, ReportDataStamp.LastChange(ReportPeriod.ForMonth(1971, 3)), "End of month is inside the period");
            Assert.AreEqual(third, ReportDataStamp.LastChange(ReportPeriod.ForMonth(1971, 4)));
            Assert.AreEqual(third, ReportDataStamp.LastChange(ReportPeriod.ForYear(1971)));
            Assert.AreEqual(0, ReportDataStamp.LastChange(ReportPeriod.ForMonth(1971, 2)));
        }

        [TestMethod]
        public void Touch_NoDates_DoesNotChangeTheStamp()
        {
            var stamp = ReportDataStamp.Current;
            ReportDataStamp.Touch(Enumerable.Empty<DateTime>());
            Assert.AreEqual(stamp, ReportDataStamp.Current);
        }
    }

    [TestClass]
    public class AvailableDatesCacheTest
    {
        private static IDictionary<DateTime, int> Dates(int count)
        {
            return new Dictionary<DateTime, int> { { new DateTime(1972, 1, 1), count } };
        }

        /// <summary>BUG-0730b3: concurrent requests used to Remove/Add on one shared Dictionary and failed with duplicate keys.</summary>
        [TestMethod]
        public void ConcurrentRequests_NoExceptions_SingleRebuild()
        {
            var key = "test-" + Guid.NewGuid().ToString("N");
            var builds = 0;
            Func<IDictionary<DateTime, int>> build = () =>
            {
                Interlocked.Increment(ref builds);
                Thread.Sleep(50); // a slow query, so the other threads pile up behind the rebuild
                return Dates(builds);
            };

            IReadOnlyDictionary<DateTime, int>[] Run()
            {
                using (var start = new Barrier(16))
                {
                    var tasks = Enumerable.Range(0, 16).Select(_ => Task.Factory.StartNew(() =>
                    {
                        start.SignalAndWait();
                        return AvailableDatesCache.GetOrBuild(key, build);
                    }, TaskCreationOptions.LongRunning)).ToArray();
                    Task.WaitAll(tasks);
                    return tasks.Select(t => t.Result).ToArray();
                }
            }

            var first = Run();
            Assert.AreEqual(1, builds, "One rebuild for 16 concurrent cold requests");
            Assert.IsTrue(first.All(r => ReferenceEquals(r, first[0])));

            ReportDataStamp.Touch(new[] { new DateTime(1972, 1, 1) });
            var second = Run();
            Assert.AreEqual(2, builds, "One rebuild after a change");
            Assert.IsTrue(second.All(r => ReferenceEquals(r, second[0])));
            Assert.AreEqual(2, second[0][new DateTime(1972, 1, 1)]);
        }

        [TestMethod]
        public void Snapshot_IsReadOnly()
        {
            var dates = AvailableDatesCache.GetOrBuild("test-" + Guid.NewGuid().ToString("N"), () => Dates(1));
            Assert.IsFalse(dates is IDictionary<DateTime, int> mutable && !mutable.IsReadOnly, "Callers cannot mutate the shared snapshot");
        }

        [TestMethod]
        public void OlderThanMaxAge_Rebuilds()
        {
            var key = "test-" + Guid.NewGuid().ToString("N");
            var now = new DateTime(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);
            var builds = 0;
            Func<IDictionary<DateTime, int>> build = () => Dates(++builds);

            AvailableDatesCache.GetOrBuild(key, build, now);
            AvailableDatesCache.GetOrBuild(key, build, now.Add(AvailableDatesCache.MaxAge).AddMinutes(-1));
            Assert.AreEqual(1, builds);
            AvailableDatesCache.GetOrBuild(key, build, now.Add(AvailableDatesCache.MaxAge).AddMinutes(1));
            Assert.AreEqual(2, builds, "The 24 h safety net rebuilds even without a change");
        }
    }
}
