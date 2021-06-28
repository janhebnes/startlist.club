using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlightJournal.Web.Aprs;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.FlightExport;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;
using Newtonsoft.Json;
using RestSharp;

namespace FlightJournal.Web.AutoExport
{
    /// <summary>
    /// Exports flights to external consumers as defined in configs
    /// </summary>
    public class AutoExporter : IDisposable
    {
        private readonly IEnumerable<Configuration.AutoExport> _autoExportConfiguration;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        public AutoExporter(IEnumerable<Configuration.AutoExport> autoExportConfiguration)
        {
            _autoExportConfiguration = autoExportConfiguration;

        }

        public void Start()
        {
            foreach (var autoExportConfig in _autoExportConfiguration)
            {
                var t = new AutoExporterTask(autoExportConfig, cts.Token);
                t.Start();
            }
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }


    internal class AutoExporterTask
    {
        private readonly Configuration.AutoExport _config;
        private readonly CancellationToken token;
        private string _authToken;

        public AutoExporterTask(Configuration.AutoExport config, CancellationToken cancellationToken)
        {
            _config = config;
            token = cancellationToken;
        }

        public void Start()
        {
            if (_config.Name.IsNullOrEmpty() || _config.PostUrl.IsNullOrEmpty())
            {
                Log.Warning($"{nameof(AutoExporterTask)} unable to start - bad config");
                return;
            }

            Task.Factory.StartNew(TaskLoop, token, TaskCreationOptions.LongRunning);
        }

        private async Task TaskLoop(object _)
        {
            var sleeper = new TokenSleeper(token);
            var interval = TimeSpan.FromMinutes(_config.IntervalInMinutes > 0 ? _config.IntervalInMinutes : 60);

            while (!token.IsCancellationRequested)
            {
                Log.Information($"{nameof(AutoExporterTask)} {_config.Name} executing");

                // Find the clubs using this config
                var db = new FlightContext();
                var clubs = db.Clubs.Where(x => _config.Name == x.AutoExportConfigName).Select(c=>c.ClubId).ToList();

                // find 'dirty' flights for this config
                var clubFlights = db.Flights.Where(f => f.Pilot != null && f.CandidateForExport && clubs.Contains(f.Pilot.ClubId)).ToList();
                if (clubFlights.Any())
                {
                    var clubFlightIds = clubFlights.Select(x => x.FlightId).ToList();
                    var trainingFlightIds = db.AppliedExercises
                        .Where(x => x.Grading != null && x.Grading.Value > 0)
                        .Where(x=> clubFlightIds.Contains(x.FlightId))
                        .Select(x => x.FlightId)
                        .Distinct().ToList();
                    var canClearFlags = true;
                    if (trainingFlightIds.Any())
                    {
                        var exporter = new FlightExporter(db);
                        var data = exporter.AsJson(exporter.CreateExportModel(clubFlights.Where(f => trainingFlightIds.Contains(f.FlightId))));

                        canClearFlags &= await UploadData(data);
                    }

                    // clear dirty flag
                    if (canClearFlags)
                    {
                        foreach (var f in clubFlights)
                        {
                            f.CandidateForExport = false;
                            db.Entry(f).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }

                Log.Information($"{nameof(AutoExporterTask)} {_config.Name} waiting for {interval}");
                sleeper.Sleep(interval);
            }
        }


        private bool TryUpload(string data)
        {
            if (_authToken.IsNullOrEmpty())
                return false;
            var c = new RestClient(_config.PostUrl) { Timeout = -1 };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Bearer {_authToken}");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", data, ParameterType.RequestBody);
            var r = c.Execute(request);
            return (r.ResponseStatus == ResponseStatus.Completed 
                    && (int) r.StatusCode >= 200 
                    && (int) r.StatusCode <= 299);
        }

        private bool TryAuth()
        {
            if (_config.TokenUrl.IsNullOrEmpty()
                || _config.Username.IsNullOrEmpty()
                || _config.Password.IsNullOrEmpty())
            {
                Log.Warning($"{nameof(AutoExporter)} unable to authorize, missing url or credentials for config {_config.Name}");
                return false;
            }

            var c = new RestClient(_config.TokenUrl);
            c.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", _config.Username);
            request.AddParameter("password", _config.Password);
            var r = c.Execute(request);
            if (r.ResponseStatus == ResponseStatus.Completed
                && (int) r.StatusCode >= 200
                && (int) r.StatusCode <= 299)
            {
                var t = JsonConvert.DeserializeAnonymousType(r.Content, new {access_token = ""});
                if (t.access_token.IsNullOrEmpty())
                {
                    Log.Warning($"{nameof(AutoExporter)} got bad auth token for config {_config.Name}");
                    return false;
                }

                _authToken = t.access_token;
                return true;
            }

            Log.Warning($"{nameof(AutoExporter)} unable to get auth token for config {_config.Name}  ({r.ResponseStatus} {r.StatusCode})");

            return false;
        }


        private async Task<bool> UploadData(string data)
        {
            return await Task.Run(() =>
            {

                if (TryUpload(data))
                {
                    return true;
                }

                if (!TryAuth())
                {
                    return false;
                }

                return TryUpload(data);
            });
        }
    }
}