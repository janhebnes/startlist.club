using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FlightJournal.Web.Models.Report
{
    /// <summary>
    /// The single invalidation signal for report data. <see cref="FlightContext.SaveChanges"/> touches the flight dates
    /// of every successful save, so caches can tell whether their data is still current without asking SQL.
    /// The stamp lives in process memory: this assumes a single web instance (no scale-out), like the APRS listener.
    /// </summary>
    public static class ReportDataStamp
    {
        private static long _current;
        private static readonly ConcurrentDictionary<DateTime, long> DateStamps = new ConcurrentDictionary<DateTime, long>();

        /// <summary>Increases on every change to any flight.</summary>
        public static long Current => Interlocked.Read(ref _current);

        public static void Touch(IEnumerable<DateTime> dates)
        {
            var distinct = dates.Select(d => d.Date).Distinct().ToList();
            if (distinct.Count == 0)
                return;

            var stamp = Interlocked.Increment(ref _current);
            foreach (var date in distinct)
                DateStamps.AddOrUpdate(date, stamp, (_, existing) => Math.Max(existing, stamp));
        }

        /// <summary>The stamp of the latest change to a flight dated within the period, or 0 when none changed since start-up.</summary>
        public static long LastChange(ReportPeriod period)
        {
            long max = 0;
            for (var date = period.Start; date < period.EndExclusive; date = date.AddDays(1))
            {
                if (DateStamps.TryGetValue(date, out var stamp) && stamp > max)
                    max = stamp;
            }
            return max;
        }
    }
}
