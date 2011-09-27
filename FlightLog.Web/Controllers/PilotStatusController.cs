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
    public class PilotStatusController : Controller
    {
        private FlightContext db = new FlightContext();

        //
        // GET: /PilotStatus/

        public ViewResult Index()
        {
            var pilotstatustypes = db.PilotStatusTypes.Include(p => p.Club);
            return View(pilotstatustypes.ToList());
        }

        //
        // GET: /PilotStatus/Details/5

        public ViewResult Details(int id)
        {
            PilotStatusType pilotstatustype = db.PilotStatusTypes.Find(id);
            return View(pilotstatustype);
        }

        //
        // GET: /PilotStatus/Create

        public ActionResult Create()
        {
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName");
            return View();
        } 

        //
        // POST: /PilotStatus/Create

        [HttpPost]
        public ActionResult Create(PilotStatusType pilotstatustype)
        {
            if (ModelState.IsValid)
            {
                db.PilotStatusTypes.Add(pilotstatustype);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilotstatustype.ClubId);
            return View(pilotstatustype);
        }
        
        //
        // GET: /PilotStatus/Edit/5
 
        public ActionResult Edit(int id)
        {
            PilotStatusType pilotstatustype = db.PilotStatusTypes.Find(id);
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilotstatustype.ClubId);
            return View(pilotstatustype);
        }

        //
        // POST: /PilotStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(PilotStatusType pilotstatustype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pilotstatustype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilotstatustype.ClubId);
            return View(pilotstatustype);
        }

        //
        // GET: /PilotStatus/Delete/5
 
        public ActionResult Delete(int id)
        {
            PilotStatusType pilotstatustype = db.PilotStatusTypes.Find(id);
            return View(pilotstatustype);
        }

        //
        // POST: /PilotStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            PilotStatusType pilotstatustype = db.PilotStatusTypes.Find(id);
            db.PilotStatusTypes.Remove(pilotstatustype);
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