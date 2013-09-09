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
    public class PilotController : Controller
    {
        private FlightContext db = new FlightContext();

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (!requestContext.HttpContext.User.IsInRole("Administrator") && !requestContext.HttpContext.User.IsInRole("Editor"))
            {
                throw new UnauthorizedAccessException(string.Format("Access Denied to User {0}", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            base.Initialize(requestContext);
        }

        //
        // GET: /Pilot/

        public ViewResult Index()
        {
            var pilots = db.Pilots.Include(p => p.Club);
            return View(pilots.ToList().Where(d=>d.Club.IsCurrent()));
        }

        //
        // GET: /Pilot/Details/5

        public ViewResult Details(int id)
        {
            Pilot pilot = db.Pilots.Find(id);
            return View(pilot);
        }

        //
        // GET: /Pilot/Create

        public ActionResult Create()
        {
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName");
            return View();
        } 

        //
        // POST: /Pilot/Create

        [HttpPost]
        public ActionResult Create(Pilot pilot)
        {
            if (ModelState.IsValid)
            {
                db.Pilots.Add(pilot);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilot.ClubId);
            return View(pilot);
        }
        
        //
        // GET: /Pilot/Edit/5
 
        public ActionResult Edit(int id)
        {
            Pilot pilot = db.Pilots.Find(id);
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilot.ClubId);
            return View(pilot);
        }

        //
        // POST: /Pilot/Edit/5

        [HttpPost]
        public ActionResult Edit(Pilot pilot)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pilot).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilot.ClubId);
            return View(pilot);
        }

        //
        // GET: /Pilot/Delete/5
 
        public ActionResult Delete(int id)
        {
            Pilot pilot = db.Pilots.Find(id);
            return View(pilot);
        }

        //
        // POST: /Pilot/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Pilot pilot = db.Pilots.Find(id);
            db.Pilots.Remove(pilot);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}