using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlightJournal.Web.Models.Report
{
    /// <summary>
    /// Flight counts per date for the report date picker, per club. Each entry is an immutable snapshot that is replaced
    /// as a whole, so concurrent requests never mutate shared state (BUG-0730b3). A snapshot is reused while no flight
    /// has changed (<see cref="ReportDataStamp"/>) and it is younger than <see cref="MaxAge"/>.
    /// </summary>
    public static class AvailableDatesCache
    {
        /// <summary>Safety net in case a change is ever missed by the stamp.</summary>
        public static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

        private sealed class Entry
        {
            public Entry(IReadOnlyDictionary<DateTime, int> dates, long builtAtStamp, DateTime builtUtc)
            {
                Dates = dates;
                BuiltAtStamp = builtAtStamp;
                BuiltUtc = builtUtc;
            }

            public IReadOnlyDictionary<DateTime, int> Dates { get; }
            public long BuiltAtStamp { get; }
            public DateTime BuiltUtc { get; }

            public bool IsFresh(DateTime utcNow) => BuiltAtStamp == ReportDataStamp.Current && utcNow - BuiltUtc < MaxAge;
        }

        private static readonly ConcurrentDictionary<string, Entry> Entries = new ConcurrentDictionary<string, Entry>();
        private static readonly ConcurrentDictionary<string, object> RebuildLocks = new ConcurrentDictionary<string, object>();

        public static IReadOnlyDictionary<DateTime, int> Get(FlightContext db, ReportClubFilter club)
        {
            return GetOrBuild(club.ShortName ?? string.Empty, () => Query(db, club));
        }

        /// <summary>Returns the fresh snapshot for the key, or builds one; concurrent callers wait for a single rebuild.</summary>
        public static IReadOnlyDictionary<DateTime, int> GetOrBuild(string key, Func<IDictionary<DateTime, int>> build, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            if (Entries.TryGetValue(key, out var entry) && entry.IsFresh(now))
                return entry.Dates;

            lock (RebuildLocks.GetOrAdd(key, _ => new object()))
            {
                if (Entries.TryGetValue(key, out entry) && entry.IsFresh(now))
                    return entry.Dates;

                // Read the stamp before querying: a change during the query leaves the snapshot stale, so the next request rebuilds
                var stamp = ReportDataStamp.Current;
                var dates = new ReadOnlyDictionary<DateTime, int>(new Dictionary<DateTime, int>(build()));
                Entries[key] = new Entry(dates, stamp, now);
                return dates;
            }
        }

        /// <summary>One GROUP BY projection: the number of non-deleted flights per date for the club.</summary>
        public static Dictionary<DateTime, int> Query(FlightContext db, ReportClubFilter club)
        {
            return ReportDataLoader.WhereClub(db.Flights.Where(f => f.Deleted == null), club)
                .GroupBy(f => f.Date)
                .Select(g => new { Date = g.Key, FlightCount = g.Count() })
                .ToDictionary(x => x.Date, x => x.FlightCount);
        }
    }
}
