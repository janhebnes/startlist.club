using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Models
{
    public class LogbookViewModel
    {
        public TrainingBarometerViewModel TrainingBarometer { get; set; }
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