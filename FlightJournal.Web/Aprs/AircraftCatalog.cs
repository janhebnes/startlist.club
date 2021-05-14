using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Logging;

namespace FlightJournal.Web.Aprs
{
    public class Aircraft
    { 
        public string Id { get; }
        public string Type { get; }
        public string Registration { get; }
        public string CompetitionId { get; }

        public Aircraft(string line)
        {
            var fields = line.Split(',').Select(x => x.Substring(1, x.Length - 2)).ToList(); // strip ''s
            Id = fields[1];
            Type = fields[2];
            Registration = fields[3];
            CompetitionId = fields[4];
        }

        public override string ToString()
        {
            return $"{Registration} ({Type}) [{CompetitionId}]";
        }

        public bool MatchesId(string anId)
        {
            return (anId.Contains(Id));
        }
    }

    public static class AircraftExtensions
    {
        public static string Info(this Aircraft a)
        {
            return a?.ToString() ?? "";
        }
    }

    public interface IAircraftCatalog : IDisposable
    {
        Aircraft AircraftInfo(string id);
    }


    public class AircraftCatalog : IAircraftCatalog
    {
        private readonly string _url;
        private List<Aircraft> _catalog = new();
        private readonly object _catalogLock = new object();
        private readonly CancellationTokenSource _cts = new();
        public string DefaultUrl => "https://ddb.glidernet.org/download";

        public AircraftCatalog(string url=null)
        {
            _url = url.IsNullOrEmpty() ? DefaultUrl : url;

            Task.Factory.StartNew(RefreshCatalog, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public void Dispose()
        {
        }

        private async Task RefreshCatalog()
        {
            while (!_cts.IsCancellationRequested)
            {
                var catalog = await LoadAircraftIdentities();
                if (!catalog.IsNullOrEmpty())
                {
                    lock (_catalogLock)
                    {
                        _catalog = catalog.ToList();
                    }
                }

                var sleeper = new TokenSleeper(_cts.Token);
                sleeper.Sleep(TimeSpan.FromHours(1));
            }
        }

        private async Task<IEnumerable<Aircraft>> LoadAircraftIdentities()
        {
            var result = new List<Aircraft>();
            try
            {
                Log.Debug("Fetching aircraft identities");
                using (var client = new HttpClient())
                using (var file = await client.GetStreamAsync(_url))
                using (var reader = new StreamReader(file))
                {
                    var line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("#"))
                            continue;
                        try
                        {
                            result.Add(new Aircraft(line));
                        }
                        catch (Exception e)
                        {
                            Log.Warning($"Failed to fetch aircraft identities: {e.Message}");
                        }
                    }
                }
                Log.Debug($"Fetched identities of {result.Count} aircraft");
            }
            catch (Exception e)
            {
                Log.Warning($"Failed to fetch aircraft identities: {e.Message}");
            }

            return result;
        }

        public Aircraft AircraftInfo(string id)
        {
            Aircraft a;
            lock (_catalogLock)
            {
                a = _catalog.FirstOrDefault(x => x.MatchesId(id));
            }

            return a;
        }

    }
}