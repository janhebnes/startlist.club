namespace FlightLog.ViewModels.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FlightLog.Models;

    public class ReportViewModel
    {
        public DateTime Date { get; set; }
        public Dictionary<DateTime, int> AvailableDates { get; set; }
        public IQueryable<Flight> Flights { get; set; }
    }
}