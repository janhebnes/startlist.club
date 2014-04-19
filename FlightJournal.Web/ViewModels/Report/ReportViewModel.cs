using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightJournal.Web.ViewModels.Report
{
    public class ReportViewModel
    {
        public DateTime Date { get; set; }
        public Dictionary<DateTime, int> AvailableDates { get; set; }
        public IQueryable<Models.Flight> Flights { get; set; }
    }
}