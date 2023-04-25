using System;
using System.Collections.Generic;

namespace FlightJournal.Web.Models
{
    public class LogbookViewModel
    {
        public TrainingBarometerViewModel TrainingBarometer { get; set; }
        public int Year { get; set; }
        public Pilot Pilot { get; set; }
        public IEnumerable<Models.Flight> Flights { get; set; }
        public bool TrainingBarometerEnabled { get; set; } = false;
    }

    public class TrainingBarometerViewModel
    {
        public int Last12MonthDepartures { get; set; }
        public long Last12MonthDuration { get; set; }
        public double Last12MonthHours => new TimeSpan(Last12MonthDuration).TotalHours;
        public string BarometerColorCode { get; set; }
        public string BarometerLabel { get; set; }
        public string BarometerRecommendation { get; set; }
        public string DateOfLastFlight { get; set; }
    }
}