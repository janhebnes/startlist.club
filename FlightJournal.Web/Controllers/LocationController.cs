using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using Microsoft.Ajax.Utilities;

namespace FlightJournal.Web.Controllers
{
    [NoCache]
    [Authorize(Roles = "Administrator,Manager,Editor")]
    public class LocationController : Controller
    {
        private FlightContext db = new FlightContext();

        //
        // GET: /Location/

        public ViewResult Index()
        {
            return View(db.Locations.OrderBy(t=>t.Name).ToList());
        }

        //
        // GET: /Location/Details/5

        public ViewResult Details(int id)
        {
            Location location = db.Locations.Find(id);
            return View(location);
        }

        //
        // GET: /Location/Create
        public ActionResult Create()
        {
            PopulateViewBag(null);
            return View();
        } 

        //
        // POST: /Location/Create

        [HttpPost]
        public ActionResult Create(Location location)
        {
            location.CreatedTimestamp = DateTime.Now;
            location.CreatedBy = User.Pilot().ToString();
            location.LastUpdatedTimestamp = DateTime.Now;
            location.LastUpdatedBy = User.Pilot().ToString();
            if (!location.ICAO.IsNullOrWhiteSpace()) location.ICAO = location.ICAO.ToUpperInvariant();
            if (ModelState.IsValid)
            {
                db.Locations.Add(location);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(location);
        }
        
        //
        // GET: /Location/Edit/5
 
        public ActionResult Edit(int id)
        {
            
            Location location = db.Locations.Find(id);
            ViewBag.UsedCount = db.Flights.Count(f => f.StartedFromId == id || f.LandedOnId == id);
            PopulateViewBag(location);
            return View(location);
        }

        //
        // POST: /Location/Edit/5

        [HttpPost]
        public ActionResult Edit(Location location)
        {
            location.LastUpdatedTimestamp = DateTime.Now;
            location.LastUpdatedBy = User.Pilot().ToString();
            if (!location.ICAO.IsNullOrWhiteSpace()) location.ICAO = location.ICAO.ToUpperInvariant();
            if (ModelState.IsValid)
            {
                db.Entry(location).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(location);
        }

        //
        // GET: /Location/Delete/5
 
        public ActionResult Delete(int id)
        {
            Location location = db.Locations.Find(id);
            if (db.Flights.Any(f => f.StartedFromId == id || f.LandedOnId == id))
            {
                return View("DeleteLocked",location);
            }

            return View(location);
        }

        //
        // POST: /Location/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Location location = db.Locations.Find(id);
            if (db.Flights.Any(f => f.StartedFromId == id || f.LandedOnId == id))
            {
                return View("DeleteLocked", location);
            }
            db.Locations.Remove(location);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void PopulateViewBag(Location location)
        {
            this.ViewBag.Country = new SelectList(GetCountriesByIso3166(), "TwoLetterISORegionName", "EnglishName", (location == null) ? (object)null : location.Country);    
        }

        /// <summary>
        /// Gets the list of countries based on ISO 3166-1
        /// </summary>
        /// <returns>Returns the list of countries based on ISO 3166-1</returns>
        public static List<RegionInfo> GetCountriesByIso3166()
        {
            List<RegionInfo> countries = new List<RegionInfo>();
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo country = new RegionInfo(culture.LCID);
                if (countries.Count(p => p.Name == country.Name) == 0)
                    countries.Add(country);
            }
            return countries.OrderBy(p => p.EnglishName).ToList();
        }
    }
}