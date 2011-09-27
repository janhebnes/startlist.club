using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlightLog.Controllers
{
    using System.Web.Helpers;

    using FlightLog.Models;
    using FlightLog.ViewModels.Report;

    public class ReportController : Controller
    {
        private FlightContext db = new FlightContext();

        // GET: /Report/
        public ActionResult Index(DateTime? date)
        {
            // URL information can be reviewed using RouteData.Values["date"]
            var raw = RouteData.Values["date"];
            int year = 0;

            // YEAR Statistics
            if (!date.HasValue && raw != null)
            {
                if (int.TryParse(raw.ToString(), out year))
                {
                    if ((DateTime.Now.Year >= year) && (year > 1990))
                    {
                        var rptYear = new ReportViewModel();
                        rptYear.Date = new DateTime(year, 1, 1);
                        rptYear.Flights = this.db.Flights.Where(f => f.Date.Year == rptYear.Date.Year).OrderBy(o => o.Departure);
                        return this.View("year", rptYear);
                    }
                }
            }

            // MONTH Statistics
            if (raw != null && raw.ToString().Length < 9 && raw.ToString().StartsWith("20"))
            {
                var rptMonth = new ReportViewModel();
                if (date.HasValue)
                {
                    rptMonth.Date = date.Value;
                }
                else
                {
                    throw new ArgumentException(string.Format("Invalid date input in url: {0}", raw));
                }

                rptMonth.Flights = this.db.Flights.Where(f => f.Date.Month == rptMonth.Date.Month && f.Date.Year == rptMonth.Date.Year).OrderBy(o => o.Departure);

                return this.View("month", rptMonth);
            }

            var rpt = new ReportViewModel();
            rpt.AvailableDates = this.AvailableDates();
            if (rpt.AvailableDates.Count > 0 && !date.HasValue)
            {
                rpt.Date = rpt.AvailableDates.Last().Key;
            }
            else if (date.HasValue)
            {
                rpt.Date = date.Value;
            }
            else 
            {
                rpt.Date = DateTime.Today;
            }

            rpt.Flights = this.db.Flights.Where(f => f.Date == rpt.Date).OrderBy(o => o.Departure);

            return this.View(rpt);
        }

        public FileContentResult Export(int year, int month)
        {
            var flights = this.db.Flights.Where(f => f.Date.Month == month && f.Date.Year == year).OrderBy(o => o.Departure);

            var csv = Enumerable.Aggregate(flights, Flight.CsvHeaders, (current, flight) => current + flight.ToCsvString());

            ////csv = csv.Replace('æ', '祥'); //(char)145
            ////csv = csv.Replace('Æ', (char)146);
            ////csv = csv.Replace('ø', (char)248);
            ////csv = csv.Replace('Ø', (char)216);
            ////csv = csv.Replace('å', (char)134);
            ////csv = csv.Replace('Å', (char)143);
            
            // Encoding Issue... 

            return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", "Startlister-" + year + "-" + month + ".csv");
        }

        public Dictionary<DateTime, int> AvailableDates()
        {
            var availableDates = this.db.Flights.GroupBy(p => p.Date).Select(
                g => new { Date = g.Key, Flights = this.db.Flights.Where(d => d.Date == g.Key) });

            return availableDates.Select(d => new { d.Date, Hours = d.Flights.Count() }).ToDictionary(
                x => x.Date, x => x.Hours);
        }
    }
}
