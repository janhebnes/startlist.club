using System;
using System.Data.Entity;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using FlightJournal.Web.Configuration;
using FlightJournal.Web.Models;
using FlightJournal.Web.Translations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlightJournal.Web.Controllers.api
{
    [RoutePrefix("api/flights")]
    public class FlightsController : ApiController

    {
        private string TheSecretKey;
        private readonly FlightContext _db;

        public FlightsController()
        {
            _db = new FlightContext();
            TheSecretKey = Settings.ApiKey;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("")]
        public IHttpActionResult GetFlights(string key, string from=null, string to=null, string updatedSince=null, bool training=false, int pilotId=-1, string unionId=null)
        {
            if(TheSecretKey == null || key != TheSecretKey) // keep at least some peekers out (TODO: proper auth)
                return Json(Array.Empty<object>());


            var flights = _db.Flights.Where(f => f.Deleted == null).Include("Pilot");

            if (from != null && DateTime.TryParseExact(from, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                flights = flights.Where(f => f.Date >= fromDate);
            if (to != null && DateTime.TryParseExact(to, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                flights = flights.Where(f => f.Date <= toDate);
            if (updatedSince != null && DateTime.TryParseExact(updatedSince, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out  var updatedSinceDate))
                flights = flights.Where(f => f.LastUpdated >= updatedSinceDate);
            if (pilotId >= 0)
                flights = flights.Where(f => f.PilotId == pilotId);
            if (unionId != null)
                flights = flights.Where(f => f.Pilot.UnionId == unionId);


            if (training)
            {
                // paranoia check, HasTrainingData was introduced during 2021  (TODO: script a DB update) - note that this has still been observed, apparently a quick user can still manage to not set HastrainingData.
                var trainingFlightIds = _db.AppliedExercises
                    .Where(x => x.Grading != null)
                    .Select(x => x.FlightId)
                    .Distinct()
                    .ToList();
                flights = flights.Where(x => x.HasTrainingData || trainingFlightIds.Contains(x.FlightId));
            }

            var models = TrainingLogHistoryController.CreateExportModel(_db, flights, "");

            return Json(JsonConvert.SerializeObject(models, Formatting.None));
        }

        public class FlightInfo
        {
            public string t;
            public string id;
        }
    }


}