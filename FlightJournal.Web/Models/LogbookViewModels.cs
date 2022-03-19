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
        public bool TrainingBarometerEnabled { get; set; } = false;

        public FlightActivityViewModel ActivityInLast12Months { get; set; }
        public FlightActivityViewModel ActivityInLast24Months { get; set; }
        public FlightActivityViewModel ActivityInLast36Months { get; set; }
        public FlightActivityViewModel InstructorActivityInLast12Months { get; set; }
        public FlightActivityViewModel InstructorActivityInLast24Months { get; set; }
        public FlightActivityViewModel InstructorActivityInLast36Months { get; set; }
    }

    public class TrainingBarometerViewModel
    {
        public int Last12MonthDepartures { get; set; }
        public long Last12MonthDuration { get; set; }
        public string BarometerColorCode { get; set; }
        public string BarometerLabel { get; set; }
        public string BarometerRecommendation { get; set; }
    }

    public class FlightActivityViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flights">Ordered by date!</param>
        /// <param name="lowerLimit">First date of interest</param>
        public FlightActivityViewModel(IEnumerable<Flight> flights, DateTime lowerLimit, DateTime firstFlight)
        {
            var flightsSinceStart = flights.ToList().Select(f => new { f.LandingCount, f.Duration.TotalHours });
            NumberOfFlights = flightsSinceStart.Sum(f => f.LandingCount);
            Hours = flightsSinceStart.Sum(f => f.TotalHours);

            if (firstFlight > lowerLimit)
            {
                PartialData = true;
                FirstFlightDate = firstFlight;
            }
        }

        public bool PartialData { get; }

        public double Hours { get; }

        public int NumberOfFlights { get; }

        public DateTime FirstFlightDate { get; }

    }

}