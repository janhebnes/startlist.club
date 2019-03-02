using FlightJournal.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightJournal.Web.Models
{
    public class ReportViewModel
    {
        public DateTime Date { get; set; }
        public Dictionary<DateTime, int> AvailableDates { get; set; }
        public IQueryable<Models.Flight> Flights { get; set; }
        public IQueryable<Models.Flight> ClubFlights
        {
            get
            {
                // faster version of Flights IsCurrentClubPilots leaving everything IQueryable
                return Flights.Where(f => ClubController.CurrentClub.ShortName == null // allowing ClubFlights to list all pilots if no filtering is active
                    || (f.Pilot != null && f.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Betaler != null && f.Betaler.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Pilot == null && f.PilotBackseat == null && f.Betaler == null)); // allowing editing of empty unknown flights assigned to the location }
            }
        }
        public IQueryable<Models.Location> DistinctLocations { get; set; }
    }
}