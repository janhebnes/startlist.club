using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FlightJournal.Web.Models.Report
{
    /// <summary>
    /// The single invalidation signal for report data: a counter bumped after every successful save that changes report
    /// data (FlightContext.SaveChanges), plus the last stamp per flight date.
    /// Per process: this assumes a single app instance, like the Application dictionary it replaces.
    /// </summary>
    public static class ReportDataStamp
    {
        private static long _current;
        private static readonly ConcurrentDictionary<DateTime, long> DateStamps = new ConcurrentDictionary<DateTime, long>();

        public static long Current => Interlocked.Read(ref _current);

        /// <summary>Flights on these dates changed. Bumps the counter only when there is at least one date.</summary>
        public static void Touch(IEnumerable<DateTime> dates)
        {
            var distinct = dates.Select(d => d.Date).Distinct().ToList();
            if (distinct.Count == 0)
                return;
            var stamp = Interlocked.Increment(ref _current);
            foreach (var date in distinct)
                DateStamps.AddOrUpdate(date, stamp, (_, existing) => Math.Max(existing, stamp));
        }

        /// <summary>Something that can move any club's counts changed (a pilot's or club's membership); no specific dates.</summary>
        public static void TouchAll()
        {
            Interlocked.Increment(ref _current);
        }

        /// <summary>The latest stamp recorded for a date inside the period, or 0.</summary>
        public static long LastChange(ReportPeriod period)
        {
            long last = 0;
            for (var date = period.Start; date < period.EndExclusive; date = date.AddDays(1))
            {
                if (DateStamps.TryGetValue(date, out var stamp) && stamp > last)
                    last = stamp;
            }
            return last;
        }
    }
}
