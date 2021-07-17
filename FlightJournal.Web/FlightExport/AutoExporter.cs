using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlightJournal.Web.Aprs;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.FlightExport
{
    public class AutoExporter : IDisposable
    {
        private readonly FlightContext _db;
        private readonly TimeSpan _interval;
        private CancellationTokenSource _cts;

        public AutoExporter(FlightContext db, TimeSpan interval)
        {
            _db = db;
            _interval = interval;
            _cts = new CancellationTokenSource();
        }
        public void Start()
        {
            if (_interval < TimeSpan.FromMinutes(1))
            {
                Log.Information($"{this} disabled");
                return;
            }

            Task.Factory.StartNew(TaskLoop, _cts.Token, TaskCreationOptions.LongRunning);

        }

        private void TaskLoop(object obj)
        {
            var sleeper = new TokenSleeper(_cts.Token);

            while (!_cts.IsCancellationRequested)
            {
                var exportingClubs = _db.Clubs
                    .Where(c => c.ExportRecipient != null)
                    .Select(c => 
                        new
                        {
                            c.ClubId,
                            c.ExportRecipientId,
                            c.Name
                        }).ToList();
                Log.Debug($"{this} checking {exportingClubs} for updated flights to export");
                foreach (var club in exportingClubs)
                {
                    ExportRecipient exportConfig = _db.ExportRecipients.SingleOrDefault(x=>x.ExportRecipientId == club.ExportRecipientId);
                    if (exportConfig == null)
                        continue;
                    var pilotIdsInClub = _db.Pilots.Where(p => p.ClubId == club.ClubId).Select(p => p.PilotId).ToList();
                    var flights = _db.Flights.Where(f =>
                        f.LastUpdated > exportConfig.LastUpdated 
                        && f.HasTrainingData
                        && (pilotIdsInClub.Contains(f.PilotId) 
                            || (f.PilotBackseatId.HasValue && pilotIdsInClub.Contains(f.PilotBackseatId.Value))))
                        .OrderBy(f=>f.LastUpdated)
                        .ToList();

                    var exporter = FlightExporterFactory.Create(_db, exportConfig);
                    Log.Debug($"{this} exporting {flights.Count} flights updated since {exportConfig.LastUpdated} from club {club.Name} to {exportConfig.Name}");

                    var chunkSize = exportConfig.MaxDeliverySize > 0 ? exportConfig.MaxDeliverySize : flights.Count;
                    var remaining = flights.AsList();
                    while (remaining.Any())
                    {
                        var part = remaining.Take(chunkSize).AsList();
                        remaining = remaining.Skip(part.Count()).AsList();
                        if (!part.Any())
                            continue;

                        var res = exporter.Export(part);
                        if (res == FlightExporter.ExportResult.SUCCESS)
                        {
                            var latest = part.Last().LastUpdated;
                            exportConfig.LastUpdated = latest;
                            Log.Debug($"{this} exported {part.Count} flights from club {club.ClubId} to {exportConfig.Name}, latest: {latest}");
                        }
                        else
                        {
                            Log.Warning($"{this} failed to export {part.Count} flights from club {club.ClubId} to {exportConfig.Name}");
                        }
                    }

                    _db.SaveChanges();
                }
                sleeper.Sleep(_interval);
            }
        }


        public void Dispose()
        {
            _cts.Cancel();
            _cts?.Dispose();
        }
    }
}