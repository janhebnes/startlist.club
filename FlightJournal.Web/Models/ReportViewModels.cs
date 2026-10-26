using FlightJournal.Web.Models.Report;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FlightJournal.Web.Models
{
    public class ReportViewModel
    {
        public DateTime Date { get; set; }

        public string FormattedDate { get; set; }

        public Dictionary<DateTime, int> AvailableDates { get; set; }

        public List<Models.Flight> Flights { get; private set; }

        public List<Models.Flight> ClubFlights { get; private set; }

        public IEnumerable<IGrouping<Club,Flight>> Clubs { get; private set; }

        public List<Models.Location> DistinctLocations { get; set; }

        /// <summary>
        /// Computes everything the report views read from the flights loaded by ReportDataLoader, in memory,
        /// so rendering a report never goes back to the database.
        /// </summary>
        public void Load(List<Flight> flights, ReportClubFilter club)
        {
            Flights = flights;

            ClubFlights = flights.Where(club.IsClubFlight).ToList();

            // Grouped by id: AsNoTracking gives each flight its own Club instance
            Clubs = ClubFlights.Where(f => f.Deleted == null && f.Pilot != null)
                .GroupBy(f => f.Pilot.ClubId)
                .Select(g => (IGrouping<Club, Flight>)new ClubGrouping(g.First().Pilot.Club, g.ToList()))
                .OrderByDescending(o => o.Count())
                .ToList();

            DistinctLocations = ClubFlights.Select(d => d.StartedFrom)
                .GroupBy(l => l.LocationId)
                .Select(g => g.First())
                .OrderBy(d => d.Name)
                .ToList();
        }

        /// <summary>The number of distinct people in either seat of the club flights matching <paramref name="where"/>.</summary>
        public int DistinctClubPilotCount(Func<Flight, bool> where = null)
        {
            var flights = where == null ? ClubFlights : ClubFlights.Where(where);
            return flights.Select(f => (int?)f.PilotId)
                .Concat(flights.Select(f => f.PilotBackseatId))
                .Where(id => id.HasValue)
                .Distinct()
                .Count();
        }

        private sealed class ClubGrouping : IGrouping<Club, Flight>
        {
            private readonly IReadOnlyList<Flight> _flights;

            public ClubGrouping(Club key, IReadOnlyList<Flight> flights)
            {
                Key = key;
                _flights = flights;
            }

            public Club Key { get; }

            public IEnumerator<Flight> GetEnumerator() => _flights.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
