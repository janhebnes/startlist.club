using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Controllers
{
    public class LogbookController : Controller
    {
        private FlightContext db = new FlightContext();

        public ActionResult Index(int? year)
        {
            if (!Request.IsPilot())
                return RedirectToAction("PilotNotFound", "Error");

            LogbookViewModel model = new LogbookViewModel();

            model.Pilot = Request.Pilot();

            model.Year = DateTime.Now.Year;
            if (year.HasValue)
            {
                if ((DateTime.Now.Year >= year) && (year > 1990))
                {
                    model.Year = year.Value;
                }
            }

            // Custom inline Pilot filtering for allowing maximum performance
            model.Flights = this.db.Flights.Where(f => f.Date.Year >= model.Year - 1 && f.Deleted == null)
                .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("PilotBackseat").Include("Betaler")
                .Where(f => (f.Pilot != null && f.Pilot.PilotId == model.Pilot.PilotId)
                    || (f.PilotBackseat != null && f.PilotBackseat.PilotId == model.Pilot.PilotId)
                    || (f.Betaler != null && f.Betaler.PilotId == model.Pilot.PilotId))
                .OrderByDescending(o => o.Departure)
                .AsQueryable();

            var last12months = DateTime.Now.AddYears(-1);
            model.TrainingBarometer = GetTrainingBarometer(model.Flights.Where(f => f.Date > last12months));

            return this.View(model);
        }

        private TrainingBarometerViewModel GetTrainingBarometer(IQueryable<Flight> last12MonthsFlights)
        {
            var result = new TrainingBarometerViewModel();
            if (last12MonthsFlights.Any())
            {
                result.Last12MonthDepartures = last12MonthsFlights.Sum(f => f.LandingCount);
                result.Last12MonthDuration = last12MonthsFlights.ToList().Sum(f => f.Duration.Ticks);
            }
            else
            {
                result.Last12MonthDepartures = 0;
                result.Last12MonthDuration = 0;
            }

            // The following section is based on the uhb599 from DSvU Træningsbarometer
            // Duration is weighted 0,75 of start amount 
            var duration = new TimeSpan(result.Last12MonthDuration).TotalHours;
            var weightedDuration = 0.75 * duration;
            var meanFlightFitnessIndex = (result.Last12MonthDepartures + weightedDuration) / 2;

            if (meanFlightFitnessIndex > 20.5)
            {
                result.BarometerColorCode = "#339933";
                result.BarometerLabel = "Grønt område";
                result.BarometerRecommendation = "Du er i god flyvetræning, men pas på !!";
            }
            else if (meanFlightFitnessIndex > 10.5)
            {
                result.BarometerColorCode = "#ffcc00";
                result.BarometerLabel = "Gult område";
                result.BarometerRecommendation = "Du er ikke så god som du tror !!";
            }
            else
            {
                result.BarometerColorCode = "#ff3300";
                result.BarometerLabel = "Rødt område";
                result.BarometerRecommendation = "Du er rusten !!";
            }

            return result;
        }
    }
}