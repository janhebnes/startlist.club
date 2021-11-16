using FlightJournal.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FlightJournal.Web.Models
{
    public class ReportViewModel
    {
        private IQueryable<Flight> _flights;
        public DateTime Date { get; set; }

        public string FormattedDate { get; set; }

        public Dictionary<DateTime, int> AvailableDates { get; set; }

        public List<Models.Flight> Flights { get; private set; }
        
        public IQueryable<Flight> QueryableFlights
        {
            get => _flights;
            set
            {
                _flights = value;
                
                Flights = value.ToList();

                QueryableClubFlights = value.Where(f =>
                    ClubController.CurrentClub.ShortName == null // allowing ClubFlights to list all pilots if no filtering is active
                    || (f.Pilot != null && f.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Betaler != null && f.Betaler.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Pilot == null && f.PilotBackseat == null && f.Betaler == null)); // allowing editing of empty unknown flights assigned to the location }
                
                ClubFlights = QueryableClubFlights.ToList();

                Clubs = QueryableClubFlights.Where(f => f.Deleted == null).GroupBy(f => f.Pilot.Club).OrderByDescending(o => o.Count());

                DistinctLocations = QueryableClubFlights.Select(d => d.StartedFrom).Distinct().OrderBy(d => d.Name).ToList();
            }
        }

        public IQueryable<Models.Flight> QueryableClubFlights { get; private set; }
        public List<Models.Flight> ClubFlights { get; private set; }

        public IEnumerable<IGrouping<Club,Flight>> Clubs { get; private set; }

        public List<Models.Location> DistinctLocations { get; set; }
    }
}