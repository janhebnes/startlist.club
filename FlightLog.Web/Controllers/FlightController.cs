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

    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class FlightController : Controller
    {
        /// <summary>
        /// The Db context for the class
        /// </summary>
        private FlightContext db = new FlightContext();

        public ViewResult Index()
        {
            var flights = this.db.Flights.Take(60).OrderByDescending(s => s.Departure ?? DateTime.Now);
            var l = flights.ToList();
            return View(l);
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
            Flight flight = this.db.Flights.Find(id);
            if ((flight != null) && (flight.Landing == null))
            {
                flight.Landing = DateTime.Now.AddMinutes(-1 * offSet.GetValueOrDefault(0));
                this.db.Entry(flight).State = EntityState.Modified;
                this.db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Set the departure time to actual time.
        /// </summary>
        /// <param name="id">Flight Id for the flight not having the landing set</param>
        /// /// <param name="offSet">Minutes to offset the time</param>
        /// <returns>Action link</returns>
        public ActionResult Depart(Guid id, int? offSet)
        {
            Flight flight = this.db.Flights.Find(id);
            if ((flight != null) && (flight.Landing == null))
            {
                flight.Departure = DateTime.Now.AddMinutes(-1 * offSet.GetValueOrDefault(0));
                this.db.Entry(flight).State = EntityState.Modified;
                this.db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Clone(Guid id)
        {
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


        public ActionResult Create()
        {
            var flight = new Flight
                {
                    Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                };
            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // POST: /Flight/Create
        [HttpPost]
        public ActionResult Create(Flight flight)
        {
            if (ModelState.IsValid)
            {
                flight.FlightId = Guid.NewGuid();
                flight.LastUpdated = DateTime.Now;
                this.db.Flights.Add(flight);
                this.db.SaveChanges();
                return RedirectToAction("Index");
            }

            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // GET: /Default1/Edit/5
        public ActionResult Edit(Guid id)
        {
            Flight flight = this.db.Flights.Find(id);
            this.PopulateViewBag(flight);
            ViewBag.ChangeHistory = this.GetChangeHistory(id);
            return View(flight);
        }

        //
        // POST: /Default1/Edit/5
        [HttpPost]
        public ActionResult Edit(Flight flight)
        {
            if (ModelState.IsValid)
            {
                this.db.Entry(flight).State = EntityState.Modified;
                flight.LastUpdated = DateTime.Now;
                this.db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ChangeHistory = this.GetChangeHistory(flight.FlightId);
            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // GET: /Default1/Delete/5
        public ActionResult Delete(Guid id)
        {
            Flight flight = this.db.Flights.Find(id);
            return View(flight);
        }

        //
        // POST: /Default1/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
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
            this.ViewBag.PlaneId = new SelectList(this.db.Planes, "PlaneId", "Registration", (flight == null) ? (object)null : flight.PlaneId);
            this.ViewBag.PilotId = new SelectList(this.db.Pilots, "PilotId", "Name", (flight == null) ? (object)null : flight.PilotId);
            this.ViewBag.PilotBackseatId = new SelectList(this.db.Pilots, "PilotId", "Name", (flight == null) ? (object)null : flight.PilotBackseatId);
            this.ViewBag.BetalerId = new SelectList(this.db.Pilots, "PilotId", "Name", (flight == null) ? (object)null : flight.BetalerId);
            this.ViewBag.StartTypeId = new SelectList(this.db.StartTypes, "StartTypeId", "Name", (flight == null) ? (object)null : flight.StartTypeId);
            this.ViewBag.StartedFromId = new SelectList(this.db.Locations, "LocationId", "Name", (flight == null) ? (object)null : flight.StartedFromId);
            this.ViewBag.LandedOnId = new SelectList(this.db.Locations, "LocationId", "Name", (flight == null) ? (object)null : flight.LandedOnId);
        }
    }
}