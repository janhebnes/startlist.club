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
    using System.ServiceModel.Security;

    public class PlaneController : Controller
    {
        private FlightContext db = new FlightContext();

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (!requestContext.HttpContext.User.IsInRole("Administrator") && !requestContext.HttpContext.User.IsInRole("Editor") && !requestContext.RouteData.Values.ContainsValue("Create"))
            {
                throw new SecurityAccessDeniedException(string.Format("Access Denied to User {0}", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            base.Initialize(requestContext);
        }

        //
        // GET: /Plane/

        public ViewResult Index()
        {
            var planes = db.Planes.Include(p => p.DefaultStartType);
            return View(planes.ToList());
        }

        //
        // GET: /Plane/Details/5

        public ViewResult Details(int id)
        {
            Plane plane = db.Planes.Find(id);
            return View(plane);
        }

        //
        // GET: /Plane/Create

        public ActionResult Create()
        {
            ViewBag.StartTypeId = new SelectList(db.StartTypes, "StartTypeId", "ShortName");
            return View();
        } 

        //
        // POST: /Plane/Create

        [HttpPost]
        public ActionResult Create(Plane plane)
        {
            if (ModelState.IsValid)
            {
                db.Planes.Add(plane);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.StartTypeId = new SelectList(db.StartTypes, "StartTypeId", "ShortName", plane.StartTypeId);
            return View(plane);
        }
        
        //
        // GET: /Plane/Edit/5
 
        public ActionResult Edit(int id)
        {
            Plane plane = db.Planes.Find(id);
            ViewBag.StartTypeId = new SelectList(db.StartTypes, "StartTypeId", "ShortName", plane.StartTypeId);
            return View(plane);
        }

        //
        // POST: /Plane/Edit/5

        [HttpPost]
        public ActionResult Edit(Plane plane)
        {
            if (ModelState.IsValid)
            {
                db.Entry(plane).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StartTypeId = new SelectList(db.StartTypes, "StartTypeId", "ShortName", plane.StartTypeId);
            return View(plane);
        }

        //
        // GET: /Plane/Delete/5
 
        public ActionResult Delete(int id)
        {
            Plane plane = db.Planes.Find(id);
            return View(plane);
        }

        //
        // POST: /Plane/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Plane plane = db.Planes.Find(id);
            db.Planes.Remove(plane);
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