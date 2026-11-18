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

        /// <summary>Non-deleted flights of the period that involve the club's people, in a deterministic order, with everything the export reads.</summary>
        public static List<Flight> LoadFlights(FlightContext db, ReportPeriod period, ReportClubFilter club)
        {
            var start = period.Start;
            var end = period.EndExclusive;
            return WhereClubPeople(db.Flights.Where(f => f.Date >= start && f.Date < end && f.Deleted == null), club)
                .Include(f => f.Plane).Include(f => f.StartType).Include(f => f.StartedFrom).Include(f => f.LandedOn)
                .Include(f => f.Pilot.Club).Include(f => f.PilotBackseat).Include(f => f.Betaler)
                .OrderBy(f => f.Departure).ThenBy(f => f.FlightId)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>ReportClubFilter.IsClubFlight in SQL: club people only, no location rule (captured values only).</summary>
        public static IQueryable<Flight> WhereClubPeople(IQueryable<Flight> flights, ReportClubFilter club)
        {
            if (club.IsUnfiltered) return flights;
            var clubId = club.ClubId;
            return flights.Where(f => (f.Pilot != null && f.Pilot.ClubId == clubId)
                                   || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                                   || (f.Betaler != null && f.Betaler.ClubId == clubId)
                                   || (f.Pilot == null && f.PilotBackseat == null && f.Betaler == null));
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
