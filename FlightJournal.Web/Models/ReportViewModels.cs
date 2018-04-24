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
        public IQueryable<Models.Location> DistinctLocations { get; set; }
    }

    public class PilotReportViewModel
    {

        public TrainingBarometerViewModel TrainingBarometer { get; set; } = new TrainingBarometerViewModel();
        public int Year { get; set; }
        public Pilot Pilot { get; set; }
        public IQueryable<Models.Flight> Flights { get; set; }
        
    }

    public class TrainingBarometerViewModel
    {
        public int Last12MonthDepartures { get; set; }
        public long Last12MonthDuration { get; set; }
        public string BarometerColorCode { get; set; }
        public string BarometerLabel { get; set; }
        public string BarometerRecommendation { get; set; }
    }
}