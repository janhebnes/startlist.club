using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using FlightJournal.Web.Configuration;
using FlightJournal.Web.Models;
using Newtonsoft.Json;

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

            var affectedFlights = flights.ToList(); // Materialize the query to avoid multiple enumerations

            if (training)
            {
                // paranoia check, HasTrainingData was introduced during 2021  (TODO: script a DB update) - note that this has still been observed, apparently a quick user can still manage to not set HastrainingData.
                var trainingFlightIds = _db.AppliedExercises
                    .Select(x => x.FlightId)
                    .Distinct()
                    .ToHashSet();
                
                affectedFlights = affectedFlights
                        .Where(x => x.HasTrainingData || trainingFlightIds.Contains(x.FlightId))
                        .ToList(); // Materialize the filtered results
            }

            var models = TrainingLogHistoryController.CreateExportModel(_db, affectedFlights, "");
            return Json(JsonConvert.SerializeObject(models, Formatting.None));
        }

        public class FlightInfo
        {
            public string t;
            public string id;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("query")]
        public IHttpActionResult GetApiQuery()
        {
            var html = @"
        <html>
            <head><title>Flights API Test Query</title></head>
            <body>
                <h1>Flights API Test</h1>
                <form method='get' action='/api/flights'>
                    <label>API Key: <input name='key' /></label><br/>
                    <label>From (yyyyMMdd): <input name='from' /></label><br/>
                    <label>To (yyyyMMdd): <input name='to' /></label><br/>
                    <label>Updated Since (yyyyMMdd): <input name='updatedSince' /></label><br/>
                    <label>Training: <input type='checkbox' name='training' value='true' /></label><br/>
                    <label>Pilot ID: <input name='pilotId' type='number' /></label><br/>
                    <label>Union ID: <input name='unionId' /></label><br/>
                    <input type='submit' value='Check API' />
                </form>
                <p>Results will be shown as JSON.</p>
            </body>
        </html>";
            return new System.Web.Http.Results.ResponseMessageResult(
                new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(html, System.Text.Encoding.UTF8, "text/html")
                });
        }
    }
}