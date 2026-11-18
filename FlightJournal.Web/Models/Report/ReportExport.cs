using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace FlightJournal.Web.Models.Report
{
    /// <summary>The year/month CSV export and the day JSON export: one query over a date range, then plain text building.</summary>
    public static class ReportExport
    {
        /// <summary>The export period, or false for a date that doesn't exist. No database access.</summary>
        public static bool TryGetPeriod(int year, int? month, int? day, out ReportPeriod period)
        {
            period = null;
            if (year < 1 || year > 9998) return false;                 // EndExclusive must stay representable
            if (!month.HasValue)
            {
                if (day.HasValue) return false;
                period = ReportPeriod.ForYear(year);
                return true;
            }
            if (month < 1 || month > 12) return false;
            if (!day.HasValue)
            {
                period = ReportPeriod.ForMonth(year, month.Value);
                return true;
            }
            if (day < 1 || day > DateTime.DaysInMonth(year, month.Value)) return false;
            period = ReportPeriod.ForDay(new DateTime(year, month.Value, day.Value));
            return true;
        }

        /// <summary>
        /// Non-deleted flights of the period that involve the club's people, in a deterministic order, with everything the export reads.
        /// <para>
        /// The flight query is a join-free projection, covered by IX_Flights_Date_StartedFromId_Deleted (every mapped column except
        /// HasTrainingData, which the export doesn't read), filtered on the club's pilot ids. Joining Pilots per flight made the
        /// optimizer seek Pilots for every row. The referenced pilots (with clubs), planes, locations and start types are loaded
        /// as small separate sets and assigned in memory: at most 6 commands for a club, 5 unfiltered, independent of the flights.
        /// </para>
        /// <para>
        /// Club rule: a flight whose pilot, back-seat pilot or payer belongs to the club. The legacy "no people" case can't occur,
        /// because PilotId and BetalerId are required.
        /// </para>
        /// </summary>
        public static List<Flight> LoadFlights(FlightContext db, ReportPeriod period, ReportClubFilter club)
        {
            var proxies = db.Configuration.ProxyCreationEnabled;
            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                List<int> clubPilotIds = null;
                if (!club.IsUnfiltered)
                {
                    var clubId = club.ClubId;
                    clubPilotIds = db.Pilots.AsNoTracking().Where(p => p.ClubId == clubId).Select(p => p.PilotId).ToList();
                    if (clubPilotIds.Count == 0)
                        return new List<Flight>();
                }

                var rows = LoadRows(db, period, clubPilotIds);
                if (rows.Count == 0)
                    return new List<Flight>();

                var pilotIds = rows.Select(r => r.PilotId).Concat(rows.Select(r => r.BetalerId))
                    .Concat(rows.Where(r => r.PilotBackseatId.HasValue).Select(r => r.PilotBackseatId.Value)).Distinct().ToList();
                var planeIds = rows.Select(r => r.PlaneId).Distinct().ToList();
                var locationIds = rows.Select(r => r.StartedFromId)
                    .Concat(rows.Where(r => r.LandedOnId.HasValue).Select(r => r.LandedOnId.Value)).Distinct().ToList();
                var startTypeIds = rows.Select(r => r.StartTypeId).Distinct().ToList();

                var pilots = db.Pilots.AsNoTracking().Include(p => p.Club).Where(p => pilotIds.Contains(p.PilotId)).ToDictionary(p => p.PilotId);
                var planes = db.Planes.AsNoTracking().Where(p => planeIds.Contains(p.PlaneId)).ToDictionary(p => p.PlaneId);
                var locations = db.Locations.AsNoTracking().Where(l => locationIds.Contains(l.LocationId)).ToDictionary(l => l.LocationId);
                var startTypes = db.StartTypes.AsNoTracking().Where(s => startTypeIds.Contains(s.StartTypeId)).ToDictionary(s => s.StartTypeId);

                return rows.Select(r => r.ToFlight(pilots, planes, locations, startTypes)).ToList();
            }
            finally
            {
                db.Configuration.ProxyCreationEnabled = proxies;
            }
        }

        /// <summary>The covered, join-free flight query: a Date range seek, club filter on the pilot id columns.</summary>
        private static List<ExportFlightRow> LoadRows(FlightContext db, ReportPeriod period, List<int> clubPilotIds)
        {
            var start = period.Start;
            var end = period.EndExclusive;
            var flights = db.Flights.AsNoTracking().Where(f => f.Date >= start && f.Date < end && f.Deleted == null);
            if (clubPilotIds != null)
                flights = flights.Where(f => clubPilotIds.Contains(f.PilotId)
                                             || (f.PilotBackseatId != null && clubPilotIds.Contains(f.PilotBackseatId.Value))
                                             || clubPilotIds.Contains(f.BetalerId));

            return flights
                .OrderBy(f => f.Departure).ThenBy(f => f.FlightId)
                .Select(f => new ExportFlightRow
                {
                    FlightId = f.FlightId, Date = f.Date, Departure = f.Departure, Landing = f.Landing,
                    LandingCount = f.LandingCount, PlaneId = f.PlaneId, PilotId = f.PilotId, PilotBackseatId = f.PilotBackseatId,
                    StartTypeId = f.StartTypeId, StartedFromId = f.StartedFromId, LandedOnId = f.LandedOnId,
                    TachoDeparture = f.TachoDeparture, TachoLanding = f.TachoLanding, TaskDistance = f.TaskDistance,
                    Description = f.Description, BetalerId = f.BetalerId, StartCost = f.StartCost, FlightCost = f.FlightCost,
                    TachoCost = f.TachoCost, Deleted = f.Deleted, LastUpdated = f.LastUpdated, LastUpdatedBy = f.LastUpdatedBy,
                    RecordKey = f.RecordKey
                })
                .ToList();
        }

        /// <summary>The exported flight columns (every mapped scalar except HasTrainingData). EF6 can project into this, not into Flight.</summary>
        private sealed class ExportFlightRow
        {
            public Guid FlightId { get; set; }
            public DateTime Date { get; set; }
            public DateTime? Departure { get; set; }
            public DateTime? Landing { get; set; }
            public int LandingCount { get; set; }
            public int PlaneId { get; set; }
            public int PilotId { get; set; }
            public int? PilotBackseatId { get; set; }
            public int StartTypeId { get; set; }
            public int StartedFromId { get; set; }
            public int? LandedOnId { get; set; }
            public double? TachoDeparture { get; set; }
            public double? TachoLanding { get; set; }
            public double? TaskDistance { get; set; }
            public string Description { get; set; }
            public int BetalerId { get; set; }
            public double StartCost { get; set; }
            public double FlightCost { get; set; }
            public double TachoCost { get; set; }
            public DateTime? Deleted { get; set; }
            public DateTime LastUpdated { get; set; }
            public string LastUpdatedBy { get; set; }
            public int RecordKey { get; set; }

            /// <summary>
            /// A plain Flight (no proxy, so no lazy loading). Date is set first, then Departure, then Landing: their setters
            /// normalise against Date and Departure, as when EF materializes in mapping order.
            /// </summary>
            public Flight ToFlight(Dictionary<int, Pilot> pilots, Dictionary<int, Plane> planes,
                Dictionary<int, Location> locations, Dictionary<int, StartType> startTypes)
            {
                var flight = new Flight { FlightId = FlightId };
                flight.Date = Date;
                flight.Departure = Departure;
                flight.Landing = Landing;
                flight.LandingCount = LandingCount;
                flight.PlaneId = PlaneId;
                flight.PilotId = PilotId;
                flight.PilotBackseatId = PilotBackseatId;
                flight.StartTypeId = StartTypeId;
                flight.StartedFromId = StartedFromId;
                flight.LandedOnId = LandedOnId;
                flight.TachoDeparture = TachoDeparture;
                flight.TachoLanding = TachoLanding;
                flight.TaskDistance = TaskDistance;
                flight.Description = Description;
                flight.BetalerId = BetalerId;
                flight.StartCost = StartCost;
                flight.FlightCost = FlightCost;
                flight.TachoCost = TachoCost;
                flight.Deleted = Deleted;
                flight.LastUpdated = LastUpdated;
                flight.LastUpdatedBy = LastUpdatedBy;
                flight.RecordKey = RecordKey;

                flight.Plane = planes[PlaneId];
                flight.StartType = startTypes[StartTypeId];
                flight.StartedFrom = locations[StartedFromId];
                flight.LandedOn = LandedOnId.HasValue ? locations[LandedOnId.Value] : null;
                flight.Pilot = pilots[PilotId];
                flight.PilotBackseat = PilotBackseatId.HasValue ? pilots[PilotBackseatId.Value] : null;
                flight.Betaler = pilots[BetalerId];
                return flight;
            }
        }

        /// <summary>The CSV text as before (header, one line per flight, transliterated), built linearly.</summary>
        public static string ToCsv(IEnumerable<Flight> flights)
        {
            var csv = new StringBuilder(Transliterate(Flight.CsvHeaders));
            foreach (var flight in flights)
                csv.Append(Transliterate(flight.ToCsvString()));
            return csv.ToString();
        }

        /// <summary>Kept from the original export: Danish letters are transliterated instead of choosing a CSV encoding.</summary>
        public static string Transliterate(string input) =>
            input.Replace("æ", "ae").Replace("Æ", "AE").Replace("ø", "oe").Replace("Ø", "OE")
                 .Replace("å", "aa").Replace("Å", "AA").Replace("é", "e");
    }
}
