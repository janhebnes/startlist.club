using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Validators;
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

        public ActionResult Index()
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            var pilots = db.Pilots.Include(p => p.Club);
            if (Request.IsClub())
            {
                var clubid = Request.Club().ClubId;
                return View(pilots.ToList().Where(d => d.ClubId == clubid).OrderBy(p => p.Name));
            }

            return View(pilots.ToList().OrderBy(p => p.Name));
        }

        //
        // GET: /Pilot/Details/5

        public ActionResult Details(int id)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            Pilot pilot = db.Pilots.Find(id);
            return View(pilot);
        }

        //
        // GET: /Pilot/Create

        public ActionResult Create()
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "Name", Request.Club().ClubId); 

            return View();
        } 

        //
        // POST: /Pilot/Create

        [HttpPost]
        public ActionResult Create(Pilot model)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            // Create the pilot on the managers club if not it is invalid
            if (!User.IsAdministrator()
                && model.ClubId != User.Pilot().ClubId)
            {
                return RedirectToAction("Restricted", "Error", new { message = "Trying to create pilot in club where you are not a manager... " });
            }

            // Resolve club information manually to avoid conflict with club route
            //model.Club = db.Clubs.Find(model.ClubId);

            if (Request.IsClub())
            {
                // There is a bug where the club from the request is bound into the resolving of the club on the pilot modelstate validation, we force this through
                // HACK: This probably means there is an evil bug hidden when we are going to save more information on club level.
                ModelState.Remove("Club");
            }

            if (ModelState.IsValid)
            {
                db.Pilots.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "Name", model.ClubId);
            return View(model);
        }
        
        //
        // GET: /Pilot/Edit/5
 
        public ActionResult Edit(int id)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            Pilot pilot = db.Pilots.Find(id);
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilot.ClubId);
            return View(pilot);
        }

        //
        // POST: /Pilot/Edit/5

        [HttpPost]
        public ActionResult Edit(Pilot pilot)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            if (Request.IsClub())
            {
                // There is a bug where the club from the request is bound into the resolving of the club on the pilot modelstate validation, we force this through
                // HACK: This probably means there is an evil bug hidden when we are going to save more information on club level.
                ModelState.Remove("Club");
            }

            if (!string.IsNullOrWhiteSpace(pilot.MobilNumber))
            {
                if (!MobilNumberValidator.IsValid(pilot.MobilNumber, false))
                {
                    ModelState.AddModelError("MobilNumber", "Invalid format, please use the format "+ Request.PhoneNumberInternationalPrefix() + "12345678");
                }
                else
                {
                    pilot.MobilNumber = MobilNumberValidator.ParseMobilNumber(pilot.MobilNumber);
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(pilot).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilot.ClubId);
            return View(pilot);
        }

        [Authorize]
        public ActionResult SetEmail(PilotSetEmailViewModel model)
        {
            Pilot pilot = this.db.Pilots.Find(model.PilotId);
            if (pilot == null)
            {
                return new JsonResult() { @Data = new { Success = false }};
            }

            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });
            if (!User.IsAdministrator() && Request.Club().ClubId != pilot.ClubId) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            pilot.Email = model.Email;
            this.db.SaveChanges();

            return new JsonResult() { @Data = new { Success = true }};
        }

        [Authorize]
        public ActionResult SetMobilNumber(PilotSetMobilNumberViewModel model)
        {
            Pilot pilot = this.db.Pilots.Find(model.PilotId);
            if (pilot == null)
            {
                return new JsonResult() { @Data = new { Success = false } };
            }

            if (!MobilNumberValidator.IsValid(model.MobilNumber))
            {
                return new JsonResult() { @Data = new { Success = false } };
            }

            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });
            if (!User.IsAdministrator() && Request.Club().ClubId != pilot.ClubId) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            pilot.MobilNumber = MobilNumberValidator.ParseMobilNumber(model.MobilNumber);
            this.db.SaveChanges();

            return new JsonResult() { @Data = new { Success = true } };
        }

        //
        // GET: /Pilot/Delete/5
 
        public ActionResult Delete(int id)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

            Pilot pilot = db.Pilots.Find(id);
            ViewBag.UsedCount = db.Flights.Count(f => f.PilotId == id || f.PilotBackseatId== id);

            return View(pilot);
        }

        //
        // POST: /Pilot/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });

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