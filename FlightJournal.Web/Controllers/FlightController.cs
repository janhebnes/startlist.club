using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Services.Description;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Controllers
{
    public class FlightController : Controller
    {
        /// <summary>
        /// The Db context for the class
        /// </summary>
        private FlightContext db = new FlightContext();

        public ViewResult Index(int? skip, int? take, int? locationid)
        {
            ViewBag.Skip = skip.HasValue ? skip.Value : 0;
            ViewBag.Take = take.HasValue ? take.Value : 60;
            ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);
            
            // Custom inline Club filtering for allowing maximum performance
            // A copy of the logic in Flight.IsCurrent(Flight arg) 
            var flights = this.db.Flights
                .Where(s => !locationid.HasValue || (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value))
                .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("PilotBackseat").Include("Betaler")
                .Where(f => ClubController.CurrentClub.ShortName == null
                    || f.StartedFromId == ClubController.CurrentClub.LocationId
                    || f.LandedOnId == ClubController.CurrentClub.LocationId
                    || (f.Pilot != null && f.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Betaler != null && f.Betaler.ClubId == ClubController.CurrentClub.ClubId))
                .OrderByDescending(s => s.Date).ThenByDescending(s => s.Departure ?? DateTime.Now).Skip((skip.HasValue ? skip.Value : 0)).Take((take.HasValue ? take.Value : 60));

            return View(flights);
        }

        // POST: /Flight/History
        [Authorize]
        public ViewResult History(int? skip, int? take, int? locationid)
        {
            ViewBag.Skip = skip.HasValue ? skip.Value : 0;
            ViewBag.Take = take.HasValue ? take.Value : 100;
            ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);

            // Custom inline Club filtering for allowing maximum performance
            // A copy of the logic in Flight.IsCurrent(Flight arg) 
            var flightshistory =
                this.db.FlightVersions.Where(s => !locationid.HasValue || (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value))
                        .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("PilotBackseat").Include("Betaler")
                .Where(f => ClubController.CurrentClub.ShortName == null
                    || f.StartedFromId == ClubController.CurrentClub.LocationId
                    || f.LandedOnId == ClubController.CurrentClub.LocationId
                    || (f.Pilot != null && f.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Betaler != null && f.Betaler.ClubId == ClubController.CurrentClub.ClubId))
                 .OrderByDescending(s => s.Created).Skip((skip.HasValue ? skip.Value : 0)).Take((take.HasValue ? take.Value : 100));

            return View(flightshistory);
        }

        public ViewResult Grid(DateTime? date, int? locationid)
        {
            ViewBag.Date = date.HasValue ? date.Value : DateTime.Today;
            ViewBag.LocationId = locationid.HasValue ? locationid.Value : 0;
            ViewBag.FilterLocationId = new SelectList(this.db.Locations, "LocationId", "Name", ViewBag.LocationId);

            var flights = this.db.Flights.Where(s => (!locationid.HasValue || (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value)) && (date.HasValue ? s.Date == date : s.Date == DateTime.Today))
                .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("PilotBackseat").Include("Betaler").Include("StartType")
                .OrderByDescending(s => s.Date).ThenByDescending(s => s.Departure ?? DateTime.Now);

            return View(flights.ToList().Where(f => f.IsCurrent()));
        }

        public ViewResult Date(DateTime date)
        {
            var flights = this.db.Flights.Where(s => s.Date == date).OrderByDescending(s => s.Departure).ToList().Where(f => f.IsCurrent());
            return View("index", flights);
        }

        public ViewResult Details(Guid id)
        {
            Flight flight = this.db.Flights.Where(f => f.FlightId == id).Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("PilotBackseat").Include("Betaler").Include("StartType").FirstOrDefault();
            ViewBag.FlightId = id;
            ViewBag.ChangeHistory = this.GetChangeHistory(id);
            return View(flight);
        }

        public ViewResult ChangeHistory(Guid id)
        {
            ViewBag.FlightId = id;
            return View("_changeHistory", this.GetChangeHistory(id));
        }

        private IEnumerable<FlightVersionHistory> GetChangeHistory(Guid id)
        {
            return this.db.FlightVersions.Where(s => s.FlightId == id)
                .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("PilotBackseat").Include("Betaler").Include("StartType")
                .OrderByDescending(s => s.Created);
        }

        /// <summary>
        /// Set the landing time to actual time.
        /// </summary>
        /// <param name="id">Flight Id for the flight not having the landing set</param>
        /// <param name="offSet">Minutes to offset the time</param>
        /// <returns>Action link</returns>
        [Authorize]
        public ActionResult Land(Guid id, int? offSet)
        {
            if (!Request.IsPilot()) return RedirectToAction("Restricted", "Error", new { message = "Pilot binding required to land planes " });

            Flight flight = this.db.Flights.Find(id);
            if ((flight != null) && (flight.Landing == null))
            {
                flight.Landing = DateTime.Now.AddMinutes(-1 * offSet.GetValueOrDefault(0));
                this.db.Entry(flight).State = EntityState.Modified;
                this.db.SaveChanges();
            }

            return RedirectToAction("Grid");
        }

        /// <summary>
        /// Set the departure time to actual time.
        /// </summary>
        /// <param name="id">Flight Id for the flight not having the landing set</param>
        /// /// <param name="offSet">Minutes to offset the time</param>
        /// <returns>Action link</returns>
        [Authorize]
        public ActionResult Depart(Guid id, int? offSet)
        {
            if (!Request.IsPilot()) return RedirectToAction("Restricted", "Error", new { message = "Pilot binding required to depart planes "});

            Flight flight = this.db.Flights.Find(id);
            if ((flight != null) && (flight.Landing == null))
            {
                flight.Departure = DateTime.Now.AddMinutes(-1 * offSet.GetValueOrDefault(0));
                this.db.Entry(flight).State = EntityState.Modified;
                this.db.SaveChanges();
            }
            return RedirectToAction("Grid");
        }

        [Authorize]
        public ActionResult Clone(Guid id)
        {
            Flight originalFlight = this.db.Flights.Find(id);
            var flight = new Flight
            {
                Date = originalFlight.Date,
                PlaneId = originalFlight.PlaneId,
                PilotId = originalFlight.PilotId,
                PilotBackseatId = originalFlight.PilotBackseatId,
                BetalerId = originalFlight.BetalerId,
                StartTypeId = originalFlight.StartTypeId,
                StartedFromId = originalFlight.StartedFromId,
                LandedOnId = originalFlight.LandedOnId
            };
            this.PopulateViewBag(flight);
            return View("Create", flight);
        }

        //
        // POST: /Flight/Create
        [HttpGet]
        [Authorize]
        public ActionResult Duplicate(Guid id)
        {
            Flight originalFlight = this.db.Flights.Find(id);
            var flight = new Flight
            {
                Date = originalFlight.Date,
                PlaneId = originalFlight.PlaneId,
                PilotId = originalFlight.PilotId,
                PilotBackseatId = originalFlight.PilotBackseatId,
                BetalerId = originalFlight.BetalerId,
                StartTypeId = originalFlight.StartTypeId,
                StartedFromId = originalFlight.StartedFromId,
                LandedOnId = originalFlight.LandedOnId
            };
    
            // Create base flight information fields
            flight.FlightId = Guid.NewGuid();
            flight.LastUpdated = DateTime.Now;
            flight.LastUpdatedBy = User.Pilot().ToString();
            this.db.Flights.Add(flight);
            this.db.SaveChanges();
            return RedirectToAction("Grid");
        }

        //
        // POST: /Flight/Create
        [HttpPost]
        [Authorize]
        public ActionResult Clone(Flight flight)
        {
            return Create(flight);
        }

        [Authorize]
        public ActionResult Create()
        {
            var flight = new Flight
                {
                    Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                };

            int val;
            if (this.Request.Cookies != null)
            {
                if (this.Request.Cookies["StartTypeId"] != null && Int32.TryParse(this.Request.Cookies["StartTypeId"].Value, out val))
                {
                    flight.StartTypeId = val;
                }
                int val2;
                if (this.Request.Cookies["StartedFromId"] != null && Int32.TryParse(Request.Cookies["StartedFromId"].Value, out val2))
                {
                    flight.StartedFromId = val2;
                }
                int val3;
                if (this.Request.Cookies["LandedOnId"] != null && Int32.TryParse(Request.Cookies["LandedOnId"].Value, out val3))
                {
                    flight.LandedOnId = val3;
                }
            }
            this.PopulateViewBag(flight);
            return View(flight);
        }

        [Authorize]
        public ActionResult SetComment(Guid id, string comment)
        {
            bool isEditable = false;
            if (User.IsManager()) { isEditable = true; }

            Flight flight = this.db.Flights.Find(id);

            if (flight != null && flight.Date.AddDays(3) >= DateTime.Now)
            {
                isEditable = true;
            }

            if (!isEditable)
            {
                // TODO: Evaluate security error handling procedure vs. error controller and generel logging model of the platform
                throw new UnauthorizedAccessException(
                    string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            if (isEditable)
            {
                flight.Description = comment;
                this.db.SaveChanges();
            }

            return RedirectToAction("Grid");
        }

        //
        // POST: /Flight/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(Flight flight)
        {
            if (ModelState.IsValid)
            {
                // Remember base information in cookies
                Response.Cookies["StartTypeId"].Value = flight.StartTypeId.ToString();
                Response.Cookies["StartTypeId"].Expires = DateTime.Now.AddDays(31);
                Response.Cookies["StartedFromId"].Value = flight.StartedFromId.ToString();
                Response.Cookies["StartedFromId"].Expires = DateTime.Now.AddDays(31);
                if (flight.LandedOnId != null)
                {
                    Response.Cookies["LandedOnId"].Value = flight.LandedOnId.Value.ToString();
                    Response.Cookies["LandedOnId"].Expires = DateTime.Now.AddDays(31);
                }
                
                // Create base flight information fields
                flight.FlightId = Guid.NewGuid();
                flight.LastUpdated = DateTime.Now;
                flight.LastUpdatedBy = User.Pilot().ToString();
                this.db.Flights.Add(flight);
                this.db.SaveChanges();

                // Help handle clearing the cache on the availableDates of the Report Index page used on frontpage. 
                if (HttpContext.Application["FlightCreated" + ClubController.CurrentClub.ShortName] == null || (DateTime)HttpContext.Application["FlightCreated" + ClubController.CurrentClub.ShortName] != DateTime.Now.Date)
                {
                    HttpContext.Application["FlightCreated" + ClubController.CurrentClub.ShortName] = DateTime.Now.Date;
                    HttpContext.Application["AvailableDates" + ClubController.CurrentClub.ShortName] = null;
                }

                return RedirectToAction("Grid");
            }
            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // GET: /Flight/Edit/5
        [Authorize]
        public ActionResult Edit(Guid id)
        {
            bool isEditable = User.IsEditor();
            
            Flight flight = this.db.Flights.Find(id);

            if (flight.Date != null && flight.Date.AddDays(3) >= DateTime.Now)
            {
                isEditable = true;
            }
            if (!isEditable)
            {
                throw new UnauthorizedAccessException(
                    string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            ViewBag.FlightId = id;
            ViewBag.ChangeHistory = this.GetChangeHistory(id);
            this.PopulateViewBag(flight);
            return View(flight);
        }

        //
        // POST: /Default1/Edit/5
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Flight flight)
        {
            bool isEditable = false;
            if (User.IsAdministrator()) { isEditable = true; }
            if (User.IsManager()) { isEditable = true; }
            if (flight.Date != null && flight.Date.AddDays(3) >= DateTime.Now)
            {
                isEditable = true;
            }
            if (!isEditable)
            {
                throw new UnauthorizedAccessException(
                    string.Format("User {0} not allowed to edit this flight", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }

            if (ModelState.IsValid)
            {
                this.db.Entry(flight).State = EntityState.Modified;
                flight.LastUpdated = DateTime.Now;
                flight.LastUpdatedBy = User.Pilot().ToString();
                this.db.SaveChanges();
                
                ViewBag.UrlReferrer = ResolveUrlReferrer();
                return RedirectPermanent(ViewBag.UrlReferrer);
                //return RedirectToAction("Grid");
            }
            ViewBag.ChangeHistory = this.GetChangeHistory(flight.FlightId);
            ViewBag.FlightId = flight.FlightId;
            this.PopulateViewBag(flight);
            return View(flight);
        }

        // GET: /Flight/Delete/5
        [Authorize]
        public ActionResult Disable(Guid id)
        {
            if (!User.IsPilot())
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            if (flight != null)
                ViewBag.ChangeHistory = this.GetChangeHistory(flight.FlightId);

            ViewBag.UrlReferrer = ResolveUrlReferrer();

            return View(flight);
        }

        private string ResolveUrlReferrer()
        {
            var key = "UrlReferrer";
            if (Request.Form[key] != null)
                return Request.Form[key];

            if (Request.QueryString[key] != null)
                return Request.QueryString[key];

            if (Request.UrlReferrer != null)
                return Request.UrlReferrer.ToString();

            return "javascript:window.history.back();";
        }

        //
        // POST: /Flight/Delete/5
        [HttpPost]
        [ActionName("Disable")]
        [Authorize]
        public ActionResult DisableConfirmed(Guid id)
        {
            if (!User.IsPilot())
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            if (flight != null)
            {
                this.db.Entry(flight).State = EntityState.Modified;
                flight.Deleted = DateTime.Now;
                flight.LastUpdated = DateTime.Now;
                flight.LastUpdatedBy = User.Pilot().ToString();
                this.db.SaveChanges();
            }
            ViewBag.UrlReferrer = ResolveUrlReferrer();
            return RedirectPermanent(ViewBag.UrlReferrer);
            //return RedirectToAction("Edit", new { id = id });
        }

        //
        // POST: /Flight/Delete/5
        [Authorize]
        public ActionResult Enable(Guid id)
        {
            if (!User.IsPilot())
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            if (flight != null)
            {
                this.db.Entry(flight).State = EntityState.Modified;
                flight.Deleted = null;
                flight.LastUpdated = DateTime.Now;
                flight.LastUpdatedBy = User.Pilot().ToString();
                this.db.SaveChanges();
            }
            ViewBag.UrlReferrer = ResolveUrlReferrer();
            return RedirectPermanent(ViewBag.UrlReferrer);
            //return RedirectToAction("Edit", new { id = id});
        }
        
        // GET: /Flight/Delete/5
        [Authorize]
        public ActionResult Delete(Guid id)
        {
            if ((!User.IsAdministrator() &&
                 !User.IsManager()))
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            if (flight != null)
                ViewBag.ChangeHistory = this.GetChangeHistory(flight.FlightId);

            return View(flight);
        }

        //
        // POST: /Flight/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [Authorize]
        public ActionResult DeleteConfirmed(Guid id)
        {
            if (!User.IsEditor())
            {
                return null;
            }

            Flight flight = this.db.Flights.Find(id);
            this.db.Flights.Remove(flight);
            this.db.SaveChanges();
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            this.db.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Default ViewBag information for populating Flight select lists
        /// </summary>
        /// <param name="flight">The flight that is context to the dropdown lists</param>
        private void PopulateViewBag(Flight flight)
        {
            //this.ViewBag.PlaneId = new SelectList(this.db.Planes.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).OrderBy(p => p.CompetitionId), "PlaneId", "RenderName", (flight == null) ? (object)null : flight.PlaneId);
            var planeList = new List<ExtendedSelectListItem>();
            foreach (var plane in this.db.Planes.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).OrderBy(p => p.CompetitionId))
            {
                string planeRegistration = plane.Registration;
                if (planeRegistration.Length == 3)
                    planeRegistration = "OY-" + planeRegistration; 

                planeList.Add(
                    new ExtendedSelectListItem()
                    {
                        Value = plane.PlaneId.ToString(),
                        Text = plane.RenderName,
                        Selected = (flight != null && flight.PlaneId == plane.PlaneId),
                        htmlAttributes = new {
                            data_registration = planeRegistration,
                            data_competitionid = plane.CompetitionId,
                            data_seats = plane.Seats,
                            data_engine = plane.Engines,
                            data_defaultStartType = (plane.DefaultStartType==null)?1:plane.DefaultStartType.StartTypeId } });
            }
            this.ViewBag.PlaneId = planeList;
            this.ViewBag.StartedFromId = new SelectList(this.db.Locations.OrderBy(p => p.Name), "LocationId", "Name", (flight == null) ? (object)null : flight.StartedFromId);
            this.ViewBag.LandedOnId = new SelectList(this.db.Locations.OrderBy(p => p.Name), "LocationId", "Name", (flight == null) ? (object)null : flight.LandedOnId);    

            if (Request.IsClub())
            {
                var clubid = Request.Club().ClubId;
                this.ViewBag.BetalerId = new SelectList(this.db.Pilots.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).ToList().Where(p => p.ClubId == clubid).OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.BetalerId);
                this.ViewBag.PilotId = new SelectList(this.db.Pilots.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).ToList().Where(p => p.ClubId == clubid).OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.PilotId);
                this.ViewBag.PilotBackseatId = new SelectList(this.db.Pilots.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).ToList().Where(p => p.ClubId == clubid).OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.PilotBackseatId);
                this.ViewBag.StartTypeId = new SelectList(this.db.StartTypes.ToList().Where(p => p.ClubId == null || p.ClubId == clubid).OrderBy(p => p.LocalizedDisplayName), "StartTypeId", "LocalizedDisplayName", (flight == null) ? (object)null : flight.StartTypeId);

                if (Request.Club().Location.RegisteredOgnFlightLogAirfield)
                {
                    var ognFlights = GetOGNFlights(flight.Date);
                    this.ViewBag.OgnFlightLog = ognFlights;
                }
            }
            else
            {
                this.ViewBag.BetalerId = new SelectList(this.db.Pilots.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).ToList().OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.BetalerId);
                this.ViewBag.PilotId = new SelectList(this.db.Pilots.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).ToList().OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.PilotId);
                this.ViewBag.PilotBackseatId = new SelectList(this.db.Pilots.Where(p => !p.ExitDate.HasValue || p.ExitDate.Value > DateTime.Today).ToList().OrderBy(p => p.Name), "PilotId", "RenderName", (flight == null) ? (object)null : flight.PilotBackseatId);
                this.ViewBag.StartTypeId = new SelectList(this.db.StartTypes.ToList().Where(p => p.ClubId == null).OrderBy(p => p.LocalizedDisplayName), "StartTypeId", "LocalizedDisplayName", (flight == null) ? (object)null : flight.StartTypeId);
            }

            // TODO: Add any pilotid or starttypeid that is actually present in the flight but not present in the selectlist !
            //if (this.ViewBag.BetalerId.() flight.BetalerId 

            // Add request context for keeping the back button alive
            ViewBag.UrlReferrer = ResolveUrlReferrer();
        }
        
        /// <summary>
        /// Used to request OGN Flight information and safekeeping a cached request for not bombarding the OGN source.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private List<OGN.FlightLog.Client.Models.Flight> GetOGNFlights(DateTime date)
        {
            var cacheKey = Request.Club().Location.ICAO + date.ToShortDateString();

            var utcOffset = GetUtcOffset(Request.Club().Location.ICAO, date);
            int timeZoneOffset = utcOffset.Hours;

            var options = new OGN.FlightLog.Client.Client.Options(Request.Club().Location.ICAO, timeZoneOffset, date);
            options.Timeout = 1500;

            // ktrax logbooks does not keep records older than 7 days
            if (DateTime.Now.Date.Subtract(date).Days <= 7)
            {
                this.ViewBag.OgnFlightLogSource = options.ToString();
                this.ViewBag.OgnFlightLogSourceCsv = options.ToCsvDownloadAddress();
            }
            ViewBag.EnableLiveView = false; 
            if (DateTime.Now.Date.Subtract(date).Days == 0)
            {
                ViewBag.EnableLiveView = true;
            }

            if (HttpContext?.Cache[cacheKey] != null)
                return HttpContext.Cache[cacheKey] as List<OGN.FlightLog.Client.Models.Flight>;

            List<OGN.FlightLog.Client.Models.Flight> ognFlights = new List<OGN.FlightLog.Client.Models.Flight>();
            try
            {
                // Request the latest live feed from https://ktrax.kisstech.ch/logbook/?id=EKKS&tz=2&day=2019-04-26&units=metric&shorthand=true&showtype=true&fstatus=all&ftype=all&disp=cs&showcrew=true in csv and parse
                ognFlights = OGN.FlightLog.Client.Client.GetFlights(options);

                // Add to cache and add with a fixed expiration. 
                HttpContext?.Cache.Add(cacheKey, ognFlights, null, DateTime.Now.AddMinutes(14), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            }
            //catch (System.IO.FileNotFoundException fileNotFound)
            //{
            //    bool emptyNotJsonParsable = fileNotFound.Message == "Error reading JObject from JsonReader. Path '', line 2, position 1.";
            //    bool emptyUnfinishedJson = fileNotFound.Message == "JsonToken EndArray is not valid for closing JsonType None. Path '', line 3, position 3."; // 

            //    // There is an expected empty result from json that is broken json
            //    if (emptyNotJsonParsable || emptyUnfinishedJson)
            //    {
            //        // Empty result page having not even json brackets... 
            //        HttpContext?.Cache.Add(cacheKey, ognFlights, null, DateTime.Now.AddMinutes(20), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            //    }
            //    else
            //    {
            //        this.ViewBag.OgnFlightLogException = jre.ToString();
            //        this.ViewBag.OgnFlightLogExceptionUrl = options.ToString();
            //    }
            //}
            catch (Exception ex)
            {
                this.ViewBag.OgnFlightLogException = ex.ToString();
                this.ViewBag.OgnFlightLogExceptionUrl = options.ToString();
            }

            return ognFlights;
        }

        internal TimeSpan GetUtcOffset(string ICAO, DateTime date)
        {
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(FindSystemTimeZoneIdByICAO(ICAO, "Central European Standard Time"));
                return cstZone.GetUtcOffset(date);
            }
            catch (TimeZoneNotFoundException)
            {
                return new TimeSpan();
            }
            catch (InvalidTimeZoneException)
            {
                return new TimeSpan();
            }
        }

        internal string FindSystemTimeZoneIdByICAO(string ICAO, string defaultResult = "Central European Standard Time")
        {
            if (ICAO.Length < 5) return defaultResult;

            // based on https://en.wikipedia.org/wiki/ICAO_airport_code and https://en.wikipedia.org/wiki/List_of_airports_by_ICAO_code:_E#EK_%E2%80%93_Denmark_and_the_Faroe_Islands
            string ICAO1 = new string(ICAO.Take(1).ToArray()).ToUpperInvariant();
            string ICAO2 = new string(ICAO.Take(2).ToArray()).ToUpperInvariant();
            string ICAO3 = new string(ICAO.Take(3).ToArray()).ToUpperInvariant();
            string ICAO4 = new string(ICAO.Take(4).ToArray()).ToUpperInvariant();


            if (ICAO1 == "E")
                return "Central European Standard Time";
            
            //TODO: When locations need this to work they can extend this location

            return defaultResult;
        }

    }
}