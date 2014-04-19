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

    public class ClubController : Controller
    {
        private FlightContext db = new FlightContext();

        public static Club CurrentClub
        {
            get
            {
                // Return Session Cache if set
                if (System.Web.HttpContext.Current.Session["CurrentClub"] != null)
                {
                    return System.Web.HttpContext.Current.Session["CurrentClub"] as Club;
                }

                // Read Cookie 
                var cookie = System.Web.HttpContext.Current.Request.Cookies.Get("CurrentClub");
                if (cookie != null)
                {
                    Club ghost = new Club();
                    using (var shortDb = new FlightContext())
                    {
                        var club = shortDb.Clubs.SingleOrDefault(d => d.ShortName == cookie.Value);
                        if (club != null)
                        {
                            ghost = new Club();
                            ghost.DefaultStartLocationId = club.DefaultStartLocationId;
                            ghost.ShortName = club.ShortName;
                            ghost.ClubId = club.ClubId;

                        }
                    }

                    // Set Session Cache
                    System.Web.HttpContext.Current.Session.Add("CurrentClub", ghost);

                    // Return Current Club
                    return ghost;
                }

                // Return Empty Club
                return new Club();
            }

            private set
            {
                // Remove cookie and cache
                if (System.Web.HttpContext.Current.Response.Cookies["CurrentClub"] != null)
                    System.Web.HttpContext.Current.Response.Cookies["CurrentClub"].Expires = DateTime.Now.AddDays(-2);

                if (System.Web.HttpContext.Current.Session["CurrentClub"] != null)
                    System.Web.HttpContext.Current.Session.Remove("CurrentClub");

                // Set Cookie and Session Cache
                if (value != null && value.ShortName != null)
                {
                    System.Web.HttpContext.Current.Response.Cookies.Add(new HttpCookie("CurrentClub", value.ShortName) { Expires = DateTime.Now.AddDays(72) });
                    System.Web.HttpContext.Current.Session.Add("CurrentClub", value);
                }
                else
                {
                    System.Web.HttpContext.Current.Response.Cookies.Add(new HttpCookie("CurrentClub", string.Empty) { Expires = DateTime.Now.AddDays(72) });
                    System.Web.HttpContext.Current.Session.Add("CurrentClub", new Club());
                }
                
            }
        }

        private void ValidateForAdminPriviledges()
        {
            if (!Request.RequestContext.HttpContext.User.IsInRole("Administrator"))
            {
                throw new SecurityAccessDeniedException(
                    string.Format(
                        "Access Denied to User {0}", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        /// <summary>
        /// Used by main layout for header switch between flight club filters
        /// </summary>
        /// <returns></returns>
        public PartialViewResult ClubSelector()
        {
            return this.PartialView(db.Clubs.OrderBy(c=>c.Name).ToList());
        }

        [HttpPost]
        public ViewResult SetCurrentClub(string shortName)
        {
            // Set Current Club
            CurrentClub = this.db.Clubs.SingleOrDefault(d => d.ShortName == shortName);    

            // Return to actual path
            if (this.Request.UrlReferrer != null && this.Request.UrlReferrer.AbsolutePath != "/Club/SetCurrentClub")
            {
                this.Response.Redirect(this.Request.UrlReferrer.AbsolutePath);
            }

            return this.View(CurrentClub);
        }

        //
        // GET: /Club/

        public ViewResult Index()
        {
            ValidateForAdminPriviledges();
            return View(db.Clubs.ToList());
        }

        //
        // GET: /Club/Details/5

        public ViewResult Details(int id)
        {
            ValidateForAdminPriviledges();
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        //
        // GET: /Club/Create

        public ActionResult Create()
        {
            ValidateForAdminPriviledges();
            return View();
        }

        //
        // POST: /Club/Create

        [HttpPost]
        public ActionResult Create(Club club)
        {
            ValidateForAdminPriviledges();
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
            ValidateForAdminPriviledges();
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        //
        // POST: /Club/Edit/5

        [HttpPost]
        public ActionResult Edit(Club club)
        {
            ValidateForAdminPriviledges();
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
            ValidateForAdminPriviledges();
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        //
        // POST: /Club/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ValidateForAdminPriviledges();
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