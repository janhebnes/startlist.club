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
    }

    public class PilotReportViewModel
    {
        public int Year { get; set; }
        public Pilot Pilot { get; set; }
        public IQueryable<Models.Flight> Flights { get; set; }
    }

}