using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace FlightJournal.Web.Models.Report
{
    /// <summary>
    /// The CSV and JSON exports of the report (GH #281): one SQL command per export, filtered and sorted in SQL,
    /// with everything the CSV reads eager loaded and the text built in linear time.
    /// </summary>
    public static class ReportExport
    {
        /// <summary>The non-deleted flights of the period that involve the club's pilots, ordered by departure.</summary>
        public static List<Flight> LoadFlights(FlightContext db, ReportPeriod period, ReportClubFilter club)
        {
            var start = period.Start;
            var end = period.EndExclusive;
            return WhereClubPilots(db.Flights.Where(f => f.Date >= start && f.Date < end && f.Deleted == null), club)
                .Include(f => f.Plane).Include(f => f.StartType).Include(f => f.StartedFrom).Include(f => f.LandedOn)
                .Include(f => f.Pilot.Club).Include(f => f.PilotBackseat).Include(f => f.Betaler)
                .OrderBy(f => f.Departure)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>Flight.IsCurrentClubPilots in SQL: no location rule, and flights without any people are kept.</summary>
        public static IQueryable<Flight> WhereClubPilots(IQueryable<Flight> flights, ReportClubFilter club)
        {
            if (club.IsUnfiltered)
                return flights;

            var clubId = club.ClubId;
            return flights.Where(f => (f.Pilot != null && f.Pilot.ClubId == clubId)
                                      || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                                      || (f.Betaler != null && f.Betaler.ClubId == clubId)
                                      || (f.Pilot == null && f.PilotBackseat == null && f.Betaler == null));
        }

        public static string ToCsv(IEnumerable<Flight> flights)
        {
            var csv = new StringBuilder(Flight.CsvHeaders);
            foreach (var flight in flights)
                csv.Append(flight.ToCsvString());
            return SafeCsv(csv.ToString());
        }

        /// <summary>
        /// HACK kept from the original export: Danish letters are transliterated instead of choosing a CSV encoding.
        /// </summary>
        public static string SafeCsv(string input)
        {
            return input.Replace("æ", "ae")
                .Replace("Æ", "AE")
                .Replace("ø", "oe")
                .Replace("Ø", "OE")
                .Replace("å", "aa")
                .Replace("Å", "AA")
                .Replace("é", "e");
        }
    }
}
