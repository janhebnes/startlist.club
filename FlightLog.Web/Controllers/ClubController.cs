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
    public class ClubController : Controller
    {
        private FlightContext db = new FlightContext();

        //
        // GET: /Club/

        public ViewResult Index()
        {
            return View(db.Clubs.ToList());
        }

        //
        // GET: /Club/Details/5

        public ViewResult Details(int id)
        {
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        //
        // GET: /Club/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Club/Create

        [HttpPost]
        public ActionResult Create(Club club)
        {
            if (ModelState.IsValid)
            {
                db.Clubs.Add(club);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(club);
        }
        
        //
        // GET: /Club/Edit/5
 
        public ActionResult Edit(int id)
        {
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        //
        // POST: /Club/Edit/5

        [HttpPost]
        public ActionResult Edit(Club club)
        {
            if (ModelState.IsValid)
            {
                db.Entry(club).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(club);
        }

        //
        // GET: /Club/Delete/5
 
        public ActionResult Delete(int id)
        {
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        //
        // POST: /Club/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Club club = db.Clubs.Find(id);
            db.Clubs.Remove(club);
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