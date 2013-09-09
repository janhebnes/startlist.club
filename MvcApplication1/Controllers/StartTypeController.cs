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
    public class StartTypeController : Controller
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
        // GET: /StartType/

        public ViewResult Index()
        {
            var starttypes = db.StartTypes.Include(s => s.Club);
            return View(starttypes.ToList());
        }

        //
        // GET: /StartType/Details/5

        public ViewResult Details(int id)
        {
            StartType starttype = db.StartTypes.Find(id);
            return View(starttype);
        }

        //
        // GET: /StartType/Create

        public ActionResult Create()
        {
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName");
            return View();
        } 

        //
        // POST: /StartType/Create

        [HttpPost]
        public ActionResult Create(StartType starttype)
        {
            if (ModelState.IsValid)
            {
                db.StartTypes.Add(starttype);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", starttype.ClubId);
            return View(starttype);
        }
        
        //
        // GET: /StartType/Edit/5
 
        public ActionResult Edit(int id)
        {
            StartType starttype = db.StartTypes.Find(id);
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", starttype.ClubId);
            return View(starttype);
        }

        //
        // POST: /StartType/Edit/5

        [HttpPost]
        public ActionResult Edit(StartType starttype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(starttype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", starttype.ClubId);
            return View(starttype);
        }

        //
        // GET: /StartType/Delete/5
 
        public ActionResult Delete(int id)
        {
            StartType starttype = db.StartTypes.Find(id);
            return View(starttype);
        }

        //
        // POST: /StartType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            StartType starttype = db.StartTypes.Find(id);
            db.StartTypes.Remove(starttype);
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