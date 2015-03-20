using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class PilotController : Controller
    {
        private FlightContext db = new FlightContext();

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


        public static Pilot CurrentUserPilot
        {
            get
            {
                return GetCurrentUserPilot();
            }
        }

        public static Pilot GetCurrentUserPilot(HttpContextBase context)
        {
            // Return Session Cache if set
            if (context.Items["CurrentUserPilot"] != null)
            {
                var ghostUserPilotSession = context.Items["CurrentUserPilot"] as Pilot;
                if (ghostUserPilotSession != null)
                {
                    return ghostUserPilotSession;
                }
            }

            if (!context.Request.IsAuthenticated) return new Pilot();

            var userManager = context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindById(context.User.Identity.GetUserId());
            Pilot ghost = new Pilot();
            if (user != null)
            {
                ghost = user.Pilot ?? new Pilot();
            }

            // Set Session Cache
            context.Items.Remove("CurrentUserPilot");
            context.Items.Add("CurrentUserPilot", ghost);

            // Return Current User Pilot
            return ghost;
        }

        public static Pilot GetCurrentUserPilot()
        {
            var context = new HttpContextWrapper(System.Web.HttpContext.Current);
            if (!context.Request.IsAuthenticated)
            {
                // Return empty object for allowing more optimize linq request on the filtered data
                return new Pilot();
            }

            return GetCurrentUserPilot(context);
        }
    }
}