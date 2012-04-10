using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FlightLog.Models;

namespace FlightLog.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Remoting.Contexts;
    using System.ServiceModel.Security;

    using FlightLog.ViewModels.Flight;

    public class FlightController : Controller
    {
        /// <summary>
        /// The Db context for the class
        /// </summary>
        private FlightContext db = new FlightContext();

        public ViewResult Index(int? skip, int? take, int? locationid)
        {
            ViewBag.Skip = skip.HasValue ? skip.Value : 0;
            ViewBag.Take = take.HasValue ? take.Value : 60;
            ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);
            var flights = this.db.Flights.Where(s => locationid.HasValue ? (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value) : true).Include("Betaler").OrderByDescending(s => s.Date).ThenByDescending(s => s.Departure ?? DateTime.Now).Skip((skip.HasValue ? skip.Value : 0)).Take((take.HasValue ? take.Value : 60));
            return View(flights.ToList());
        }

        public ViewResult Sample(int? skip, int? take, int? locationid)
        {
            var initialState = new[] {
                new FlightViewModel { Title = "Tall Hat", Price = 49.95 },
                new FlightViewModel { Title = "Long Cloak", Price = 78.25 }
            };
            return View(initialState);

            //ViewBag.Skip = skip.HasValue ? skip.Value : 0;
            //ViewBag.Take = take.HasValue ? take.Value : 60;
            //ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            //ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);
            //var flights = this.db.Flights.Where(s => locationid.HasValue ? (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value) : true).OrderByDescending(s => s.Date).ThenByDescending(s => s.Departure ?? DateTime.Now).Skip((skip.HasValue ? skip.Value : 0)).Take((take.HasValue ? take.Value : 30));
            //var l = flights.ToList().Select(x => new FlightViewModel(x));
            //return View(l);
        }

        // POST: /Flight/History
        public ViewResult History(int? skip, int? take, int? locationid)
        {
            if (!Request.IsAuthenticated) 
                return null;

            ViewBag.Skip = skip.HasValue ? skip.Value : 0;
            ViewBag.Take = take.HasValue ? take.Value : 100;
            ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);
            var flightshistory =
                this.db.FlightVersions.Where(
                    s =>
                    locationid.HasValue
                        ? (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value)
                        : true).OrderByDescending(s => s.Created).Skip(
                            (skip.HasValue ? skip.Value : 0)).Take((take.HasValue ? take.Value : 100));

            return View(flightshistory.ToList());
        }

        public ViewResult Grid(DateTime? date, int? locationid)
        {
            ViewBag.Date = date.HasValue ? date.Value : DateTime.Today;
            ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);
            var flights = this.db.Flights.Where(s => 
                (locationid.HasValue ? (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value) : true) &&
                (date.HasValue ? s.Date == date : s.Date == DateTime.Today)).Include("Betaler").OrderByDescending(s => s.Date).
                ThenByDescending(s => s.Departure ?? DateTime.Now);
            return View(flights.ToList());
        }

        public ViewResult Date(DateTime date)
        {
            var flights = this.db.Flights.Where(s => s.Date == date).Take(100).OrderByDescending(s => s.Departure);
            var l = flights.ToList();
            return View("index", l);
        }

        public ViewResult Details(Guid id)
        {
            Flight flight = this.db.Flights.Find(id);
            ViewBag.FlightId = id;
            ViewBag.ChangeHistory = this.GetChangeHistory(id);
            return View(flight);
        }

        public ViewResult ChangeHistory(Guid id)
        {
            ViewBag.FlightId = id;
            return View("_changeHistory", this.GetChangeHistory(id));
        }

        private IEnumerable<FlightLog.Models.FlightVersionHistory> GetChangeHistory(Guid id)
        {
            return this.db.FlightVersions.Where(s => s.FlightId == id).OrderByDescending(s => s.Created);
        }

        /// <summary>
        /// Set the landing time to actual time.
        /// </summary>
        /// <param name="id">Flight Id for the flight not having the landing set</param>
        /// <param name="offSet">Minutes to offset the time</param>
        /// <returns>Action link</returns>
        public ActionResult Land(Guid id, int? offSet)
        {
            if (!Request.IsAuthenticated) return null;

            Flight flight = this.db.Flights.Find(id);
            if ((flight != null) && (flight.Landing == null))
            {
                flight.Landing = DateTime.Now.AddMinutes(-1 * offSet.GetValueOrDefault(0));
                this.db.Entry(flight).State = EntityState.Modified;
                this.db.SaveChanges();
            }

            return RedirectToAction("Grid");
        }

        /// <summary>
        /// Set the departure time to actual time.
        /// </summary>
        /// <param name="id">Flight Id for the flight not having the landing set</param>
        /// /// <param name="offSet">Minutes to offset the time</param>
        /// <returns>Action link</returns>
        public ActionResult Depart(Guid id, int? offSet)
        {
            if (!Request.IsAuthenticated) return null;

            Flight flight = this.db.Flights.Find(id);
            if ((flight != null) && (flight.Landing == null))
            {
                flight.Departure = DateTime.Now.AddMinutes(-1 * offSet.GetValueOrDefault(0));
                this.db.Entry(flight).State = EntityState.Modified;
                this.db.SaveChanges();
            }
            return RedirectToAction("Grid");
        }

        public ActionResult Clone(Guid id)
        {
            if (!Request.IsAuthenticated) return null;

            Flight originalFlight = this.db.Flights.Find(id);
            var flight = new Flight
            {
                Date = originalFlight.Date,
                PlaneId = originalFlight.PlaneId,
                PilotId = originalFlight.PilotId,
                PilotBackseatId = originalFlight.PilotBackseatId,
                BetalerId = originalFlight.BetalerId,
                StartTypeId = originalFlight.StartTypeId,
                StartedFromId = originalFlight.StartedFromId,
                LandedOnId = originalFlight.LandedOnId
            };
            this.PopulateViewBag(flight);
            return View("Create", flight);
        }
        //
        // POST: /Flight/Create
        [HttpPost]
        public ActionResult Clone(Flight flight)
        {
            return Create(flight);
        }

        public ActionResult Create()
        {
            if (!Request.IsAuthenticated) return null;

            var flight = new Flight
                {
                    Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                };

            int val;
            if (this.Request.Cookies != null)
            {
                if (this.Request.Cookies["StartTypeId"] != null && Int32.TryParse(this.Request.Cookies["StartTypeId"].Value, out val))
                {
                    flight.StartTypeId = val;
                }
                int val2;
                if (this.Request.Cookies["StartedFromId"] != null && Int32.TryParse(Request.Cookies["StartedFromId"].Value, out val2))
                {
                    flight.StartedFromId = val2;
                }
                int val3;
                if (this.Request.Cookies["LandedOnId"] != null && Int32.TryParse(Request.Cookies["LandedOnId"].Value, out val3))
                {
                    flight.LandedOnId = val3;
                }
            }
            this.PopulateViewBag(flight);
            return View(flight);
        }

        [HttpGet]
        public JsonResult SetComment(Guid id, string comment)
        {
            if (!Request.IsAuthenticated) return null;
            bool isEditable = false;
            if (Request.RequestContext.HttpContext.User.IsInRole("Administrator")) { isEditable = true; }
            if (Request.RequestContext.HttpContext.User.IsInRole("Editor")) { isEditable = true; }

            Flight flight = this.db.Flights.Find(id);

            if (flight.Date != null && flight.Date.AddDays(3) >= DateTime.Now)
            {
                isEditable = true;
            }
            if (!isEditable)
            {
                return Json(false, string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));

                throw new SecurityAccessDeniedException(
                    string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            if (isEditable)
            {
                flight.Description = comment;
                this.db.SaveChanges();
                return Json(true, "Succesfull saved.");
            }
            return Json(false, "Unsuccessfull");
        }

        //
        // POST: /Flight/Create
        [HttpPost]
        public ActionResult Create(Flight flight)
        {
            if (!Request.IsAuthenticated) return null;

            if (ModelState.IsValid)
            {
                // Remember base information in cookies
                Response.Cookies["StartTypeId"].Value = flight.StartTypeId.ToString();
                Response.Cookies["StartTypeId"].Expires = DateTime.Now.AddDays(31);
                Response.Cookies["StartedFromId"].Value = flight.StartedFromId.ToString();
                Response.Cookies["StartedFromId"].Expires = DateTime.Now.AddDays(31);
                if (flight.LandedOnId != null)
                {
                    Response.Cookies["LandedOnId"].Value = flight.LandedOnId.Value.ToString();
                    Response.Cookies["LandedOnId"].Expires = DateTime.Now.AddDays(31);
                }
                
                // Create base flight information fields
                flight.FlightId = Guid.NewGuid();
                flight.LastUpdated = DateTime.Now;
                flight.LastUpdatedBy = Request.RequestContext.HttpContext.User.Identity.Name;
                this.db.Flights.Add(flight);
                this.db.SaveChanges();
                return RedirectToAction("Grid");
            }

            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // GET: /Flight/Edit/5
        public ActionResult Edit(Guid id)
        {
            if (!Request.IsAuthenticated) return null;
            bool isEditable = false;
            if (Request.RequestContext.HttpContext.User.IsInRole("Administrator")) { isEditable = true; }
            if (Request.RequestContext.HttpContext.User.IsInRole("Editor")) { isEditable = true; }
            
            Flight flight = this.db.Flights.Find(id);

            if (flight.Date != null && flight.Date.AddDays(3) >= DateTime.Now)
            {
                isEditable = true;
            }
            if (!isEditable)
            {
                throw new SecurityAccessDeniedException(
                    string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            this.PopulateViewBag(flight);
            ViewBag.FlightId = id;
            ViewBag.ChangeHistory = this.GetChangeHistory(id);
            return View(flight);
        }

        //
        // POST: /Default1/Edit/5
        [HttpPost]
        public ActionResult Edit(Flight flight)
        {
            if (!Request.IsAuthenticated) return null;
            bool isEditable = false;
            if (Request.RequestContext.HttpContext.User.IsInRole("Administrator")) { isEditable = true; }
            if (Request.RequestContext.HttpContext.User.IsInRole("Editor")) { isEditable = true; }
            if (flight.Date != null && flight.Date.AddDays(3) >= DateTime.Now)
            {
                isEditable = true;
            }
            if (!isEditable)
            {
                throw new SecurityAccessDeniedException(
                    string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            if (ModelState.IsValid)
            {
                this.db.Entry(flight).State = EntityState.Modified;
                flight.LastUpdated = DateTime.Now;
                flight.LastUpdatedBy = Request.RequestContext.HttpContext.User.Identity.Name;
                this.db.SaveChanges();
                return RedirectToAction("Grid");
            }
            ViewBag.ChangeHistory = this.GetChangeHistory(flight.FlightId);
            ViewBag.FlightId = flight.FlightId;
            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // GET: /Flight/Delete/5
        public ActionResult Delete(Guid id)
        {
            if (!Request.IsAuthenticated || 
                (!Request.RequestContext.HttpContext.User.IsInRole("Editor") &&
                 !Request.RequestContext.HttpContext.User.IsInRole("Administrator")))
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            if (flight != null)
                ViewBag.ChangeHistory = this.GetChangeHistory(flight.FlightId);

            return View(flight);
        }

        //
        // POST: /Flight/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            if (!Request.IsAuthenticated ||
                (!Request.RequestContext.HttpContext.User.IsInRole("Editor") &&
                 !Request.RequestContext.HttpContext.User.IsInRole("Administrator")))
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            this.db.Flights.Remove(flight);
            this.db.SaveChanges();
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            this.db.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Default ViewBag information for populating Flight select lists
        /// </summary>
        /// <param name="flight">The flight that is context to the dropdown lists</param>
        private void PopulateViewBag(Flight flight)
        {
            this.ViewBag.PlaneId = new SelectList(this.db.Planes.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).OrderBy(p => p.CompetitionId), "PlaneId", "RenderName", (flight == null) ? (object)null : flight.PlaneId);
            this.ViewBag.BetalerId = new SelectList(this.db.Pilots.OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.BetalerId);
            this.ViewBag.PilotId = new SelectList(this.db.Pilots.OrderBy(p=>p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.PilotId);
            this.ViewBag.PilotBackseatId = new SelectList(this.db.Pilots.OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.PilotBackseatId);
            this.ViewBag.StartTypeId = new SelectList(this.db.StartTypes.OrderBy(p => p.Name), "StartTypeId", "Name", (flight == null) ? (object)null : flight.StartTypeId);
            this.ViewBag.StartedFromId = new SelectList(this.db.Locations.OrderBy(p => p.Name), "LocationId", "Name", (flight == null) ? (object)null : flight.StartedFromId);
            this.ViewBag.LandedOnId = new SelectList(this.db.Locations.OrderBy(p => p.Name), "LocationId", "Name", (flight == null) ? (object)null : flight.LandedOnId);
        }
    }
}