using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FlightJournal.Web.Models.Report
{
    public enum ReportPeriodKind { Day, Month, Year }

    /// <summary>A report period as a half-open date range, so the flight query can seek on the Date index.</summary>
    public sealed class ReportPeriod
    {
        private ReportPeriod(ReportPeriodKind kind, DateTime start, DateTime endExclusive)
        {
            Kind = kind;
            Start = start;
            EndExclusive = endExclusive;
        }

        public ReportPeriodKind Kind { get; }

        /// <summary>Inclusive, midnight.</summary>
        public DateTime Start { get; }

        /// <summary>Day: +1 day, Month: +1 month, Year: +1 year.</summary>
        public DateTime EndExclusive { get; }

        public static ReportPeriod ForDay(DateTime date) => new ReportPeriod(ReportPeriodKind.Day, date.Date, date.Date.AddDays(1));

        public static ReportPeriod ForMonth(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            return new ReportPeriod(ReportPeriodKind.Month, start, start.AddMonths(1));
        }

        public static ReportPeriod ForYear(int year)
        {
            var start = new DateTime(year, 1, 1);
            return new ReportPeriod(ReportPeriodKind.Year, start, start.AddYears(1));
        }
    }

    /// <summary>The current club, captured once per request so queries use plain values instead of ClubController.CurrentClub.</summary>
    public sealed class ReportClubFilter
    {
        public static readonly ReportClubFilter Unfiltered = new ReportClubFilter(null, 0, 0);

        private ReportClubFilter(string shortName, int clubId, int locationId)
        {
            ShortName = shortName;
            ClubId = clubId;
            LocationId = locationId;
        }

        public string ShortName { get; }
        public int ClubId { get; }
        public int LocationId { get; }
        public bool IsUnfiltered => ShortName == null;

        public static ReportClubFilter From(Club club)
        {
            return club?.ShortName == null ? Unfiltered : new ReportClubFilter(club.ShortName, club.ClubId, club.LocationId);
        }

        /// <summary>The club part of a report: flights where a club member flew or paid (or nobody is set), or everything when unfiltered.</summary>
        public bool IsClubFlight(Flight f)
        {
            return IsUnfiltered
                   || (f.Pilot != null && f.Pilot.ClubId == ClubId)
                   || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubId)
                   || (f.Betaler != null && f.Betaler.ClubId == ClubId)
                   || (f.Pilot == null && f.PilotBackseat == null && f.Betaler == null); // allowing editing of empty unknown flights assigned to the location
        }
    }

    public static class ReportDataLoader
    {
        /// <summary>One SQL round trip: the flights of the period, filtered by club, with every navigation the report views read.</summary>
        public static List<Flight> LoadFlights(FlightContext db, ReportPeriod period, ReportClubFilter club, bool excludeDeleted)
        {
            var start = period.Start;
            var end = period.EndExclusive;
            var flights = db.Flights.Where(f => f.Date >= start && f.Date < end);

            if (excludeDeleted)
                flights = flights.Where(f => f.Deleted == null);

            if (!club.IsUnfiltered)
            {
                // A copy of the logic in Flight.IsCurrent(Flight arg)
                var clubId = club.ClubId;
                var locationId = club.LocationId;
                flights = flights.Where(f => f.StartedFromId == locationId
                                             || f.LandedOnId == locationId
                                             || (f.Pilot != null && f.Pilot.ClubId == clubId)
                                             || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                                             || (f.Betaler != null && f.Betaler.ClubId == clubId));
            }

            return flights
                .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("Pilot.Club")
                .Include("PilotBackseat").Include("PilotBackseat.Club").Include("Betaler").Include("StartType")
                .OrderBy(o => o.Departure)
                .AsNoTracking()
                .ToList();
        }
    }
}
