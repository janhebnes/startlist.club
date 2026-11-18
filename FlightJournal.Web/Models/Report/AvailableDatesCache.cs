using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlightJournal.Web.Models.Report
{
    /// <summary>
    /// The report pages' available dates (flight count per date) per club: an immutable snapshot, rebuilt by one query when
    /// the report data stamp has changed or the snapshot is older than <see cref="MaxAge"/>. One rebuild per club at a time.
    /// Per process: this assumes a single app instance (no autoscale), like the Application dictionary it replaces.
    /// </summary>
    public static class AvailableDatesCache
    {
        /// <summary>A safety net for writers outside FlightContext and for multiple instances; the stamp is the primary invalidation.</summary>
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
        }

        private static readonly ConcurrentDictionary<string, Entry> Entries = new ConcurrentDictionary<string, Entry>();
        private static readonly ConcurrentDictionary<string, object> Locks = new ConcurrentDictionary<string, object>();

        public static IReadOnlyDictionary<DateTime, int> Get(FlightContext db, ReportClubFilter club)
        {
            return GetOrBuild(club.ShortName ?? "", () => Query(db, club), DateTime.UtcNow);
        }

        internal static IReadOnlyDictionary<DateTime, int> GetOrBuild(string key, Func<IDictionary<DateTime, int>> build, DateTime utcNow)
        {
            if (Entries.TryGetValue(key, out var entry) && IsFresh(entry, utcNow))
                return entry.Dates;

            // No await on this path, so a plain lock: waiters for the same club wait for one rebuild; other clubs aren't blocked
            lock (Locks.GetOrAdd(key, _ => new object()))
            {
                if (Entries.TryGetValue(key, out entry) && IsFresh(entry, utcNow))
                    return entry.Dates;

                // Read the stamp before the query: a change during the build makes the next request rebuild again
                var stamp = ReportDataStamp.Current;
                var dates = new ReadOnlyDictionary<DateTime, int>(new Dictionary<DateTime, int>(build()));
                Entries[key] = new Entry(dates, stamp, utcNow);
                return dates;
            }
        }

        private static bool IsFresh(Entry entry, DateTime utcNow)
        {
            return entry.BuiltAtStamp == ReportDataStamp.Current && utcNow - entry.BuiltUtc < MaxAge;
        }

        internal static void ResetForTests()
        {
            Entries.Clear();
            Locks.Clear();
        }

        /// <summary>One GROUP BY Date projection over non-deleted flights with the report's club predicate (location rule included).</summary>
        public static Dictionary<DateTime, int> Query(FlightContext db, ReportClubFilter club)
        {
            return ReportDataLoader.WhereClub(db.Flights.Where(f => f.Deleted == null), club)
                .GroupBy(f => f.Date)
                .Select(g => new { Date = g.Key, FlightCount = g.Count() })
                .ToDictionary(x => x.Date, x => x.FlightCount);
        }
    }
}
