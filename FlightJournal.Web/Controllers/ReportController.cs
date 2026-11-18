using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Report;

namespace FlightJournal.Web.Controllers
{
    public class ReportController : Controller
    {
        private FlightContext db = new FlightContext();

        /// <summary>
        /// If Route with club is used the club id is available directly from the method, it is only shown here for example as the CurrentClub method handles returning the current club in all contexts
        /// </summary>
        /// <param name="date"></param>
        /// <param name="club"></param>
        /// <returns></returns>
        public ActionResult Index(DateTime? date, string club = null)
        {
            // URL information can be reviewed using RouteData.Values["date"]
            var raw = RouteData.Values["date"];
            int year = 0;

            // Captured once, so the flight query uses plain values instead of re-resolving the current club
            var clubFilter = ReportClubFilter.From(ClubController.CurrentClub);

            // YEAR Statistics
            if (!date.HasValue && raw != null)
            {
                if (int.TryParse(raw.ToString(), out year))
                {
                    if ((DateTime.Now.Year >= year) && (year > 1990))
                    {
                        var rptYear = new ReportViewModel();
                        rptYear.Date = new DateTime(year, 1, 1);
                        rptYear.FormattedDate = rptYear.Date.ToString("yyyy");
                        rptYear.Load(ReportDataLoader.LoadFlights(this.db, ReportPeriod.ForYear(year), clubFilter, excludeDeleted: true), clubFilter);

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
                    rptMonth.FormattedDate = rptMonth.Date.ToString("MMMM yyyy");
                }
                else
                {
                    throw new ArgumentException(string.Format("Invalid date input in url: {0}", raw));
                }

                rptMonth.Load(ReportDataLoader.LoadFlights(this.db, ReportPeriod.ForMonth(rptMonth.Date.Year, rptMonth.Date.Month), clubFilter, excludeDeleted: true), clubFilter);

                return this.View("month", rptMonth);
            }
            
            var rpt = new ReportViewModel();
            rpt.AvailableDates = AvailableDatesCache.Get(this.db, clubFilter);
            if (rpt.AvailableDates.Count > 0
                && !date.HasValue)
            {
                rpt.Date = rpt.AvailableDates.Max(d => d.Key);
            }
            else if (date.HasValue)
            {
                rpt.Date = date.Value;
            }
            else
            {
                rpt.Date = DateTime.Today;
            }

            rpt.FormattedDate = rpt.Date.ToShortDateString();

            // Allow f.Deleted != null
            rpt.Load(ReportDataLoader.LoadFlights(this.db, ReportPeriod.ForDay(rpt.Date), clubFilter, excludeDeleted: false), clubFilter);

            return this.View(rpt);
        }

        public ActionResult Export(int year, int? month)
        {
            if (!ReportExport.TryGetPeriod(year, month, null, out var period))
                return HttpNotFound();
            var flights = ReportExport.LoadFlights(this.db, period, ReportClubFilter.From(ClubController.CurrentClub));
            var fileName = month.HasValue ? "Startlist-" + year + "-" + month + ".csv" : "Startlist-" + year + ".csv";
            return File(new System.Text.UTF8Encoding().GetBytes(ReportExport.ToCsv(flights)), "text/csv", fileName);
        }

        public ActionResult ExportJson(int year, int month, int day)
        {
            if (!ReportExport.TryGetPeriod(year, month, day, out var period))
                return HttpNotFound();
            var flights = ReportExport.LoadFlights(this.db, period, ReportClubFilter.From(ClubController.CurrentClub));
            return Json(flights.Select(f => f.ToJsonObject()).ToList(), JsonRequestBehavior.AllowGet);
        }

        internal DateTime GetCentralEuropeStandardTimeNow()
        {
            DateTime timeUtc = DateTime.UtcNow;
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                DateTime cestTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
                return cestTime;

            }
            catch (TimeZoneNotFoundException tzex)
            {
                throw new Exception("The registry does not define the Central European Standard Time zone.", tzex);
            }
            catch (InvalidTimeZoneException ivex)
            {
                throw new Exception("Registry data on the Central European Standard Time zone has been corrupted.", ivex);
            }
        }
    }
}
