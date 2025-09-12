﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Validators;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OGN.FlightLog.Client.Models;
using EntityState = System.Data.Entity.EntityState;

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
            var trainingProgramsForPilot = db.PilotsInTrainingPrograms.Where(x => x.PilotId == pilot.PilotId);
            var vm = new PilotDetailsViewModel(pilot, trainingProgramsForPilot);
            return View(vm);
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

            var trainingProgramsForPilot = db.PilotsInTrainingPrograms.Where(x => x.PilotId == pilot.PilotId);
            var vm = new PilotDetailsViewModel(pilot, trainingProgramsForPilot);

            return View(vm);
        }

        //
        // POST: /Pilot/Edit/5

        [HttpPost]
        public ActionResult Edit(PilotDetailsViewModel vm)
        {
            if (!User.IsManager()) return RedirectToAction("Restricted", "Error", new { message = "Restricted to your own club" });



            if (Request.IsClub())
            {
                // There is a bug where the club from the request is bound into the resolving of the club on the pilot modelstate validation, we force this through
                // HACK: This probably means there is an evil bug hidden when we are going to save more information on club level.
                ModelState.Remove("Club");
            }

            var pilotInDb = db.Pilots.SingleOrDefault(x => x.PilotId == vm.PilotId); 
            if(pilotInDb == null)
                return RedirectToAction("Index");


            if (!string.IsNullOrWhiteSpace(vm.MobilNumber))
            {
                if (!MobilNumberValidator.IsValid(vm.MobilNumber, false))
                {
                    ModelState.AddModelError("MobilNumber", "Invalid format, please use the format "+ Request.PhoneNumberInternationalPrefix() + "12345678");
                }
                else
                {
                    pilotInDb.MobilNumber = MobilNumberValidator.ParseMobilNumber(vm.MobilNumber);
                }
            }

            if (ModelState.IsValid)
            {
                if (pilotInDb.UnionId != vm.UnionId)
                {
                    // mark all (training) flights where the pilot participated as changed - will ensure re-export to FA
                    var affectedFlights = db.Flights.Where(f =>
                        f.Deleted == null 
                        && (f.PilotId == vm.PilotId 
                            || (f.PilotBackseatId.HasValue && f.PilotBackseatId.Value == vm.PilotId)));
                    // paranoia check, HasTrainingData was introduced during 2021  (TODO: script a DB update) - note that this has still been observed, apparently a quick user can still manage to not set HastrainingData.
                    var trainingFlightIds = db.AppliedExercises
                        .Select(x => x.FlightId)
                        .Distinct()
                        .ToHashSet();
                    affectedFlights = affectedFlights.Where(x => x.HasTrainingData || trainingFlightIds.Contains(x.FlightId));
                    foreach (var f in affectedFlights)
                    {
                        db.Entry(f).State = EntityState.Modified;
                        f.HasTrainingData = true;
                        f.LastUpdated = DateTime.Now;
                        f.LastUpdatedBy = User.Pilot().ToString();
                        // don't notify - that would probably create too much noise
                        // FlightsHub.NotifyFlightChanged(f.FlightId, Guid.Empty);
                    }
                }

                vm.Update(pilotInDb);

                db.Entry(pilotInDb).State = EntityState.Modified;
                db.SaveChanges();

                if (!vm.TrainingPrograms.IsNullOrEmpty())
                {
                    foreach (var vmpitp in vm.TrainingPrograms)
                    {
                        var dbtp = db.PilotsInTrainingPrograms.Where(x => x.PilotId == pilotInDb.PilotId && x.Training2ProgramId == vmpitp.ProgramId);
                        foreach (var pitp in dbtp)
                        {
                            vmpitp.Update(pitp);
                            db.Entry(pitp).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ShortName", pilotInDb.ClubId);
            return View(vm);
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