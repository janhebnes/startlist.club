using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FlightJournal.Web.Aprs;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;

namespace FlightJournal.Web
{
    public partial class Startup
    {
        private IAprsListener _aprsListener;
        private IAircraftCatalog _catalog;
        private AircraftEventHandler _aircraftEventHandler;

        public void ConfigureAprsDataListener()
        {
            if (!IsAprsListenerEnabled())
            {
                Log.Information("APRS data listener disabled by the AprsListenerEnabled app setting");
                return;
            }

            var tc = new Microsoft.ApplicationInsights.TelemetryClient();
            Log.Information("Setting up APRS data listener chain");

            List<ListenerArea> areas;
            using (var db = new FlightContext())
            {
                areas = db.ListenerAreas.ToList();
            }

            _catalog = new AircraftCatalog(null, tc);
            var worker = new FlightAnalysisWorker(
                () => new SkyhopFlightAnalysisEngine(),
                tc,
                () => DateTime.UtcNow,
                new FlightAnalysisWorkerOptions
                {
                    RecycleHourUtc = AprsRecycleHourUtc(),
                    ContextIdleExpiry = AprsContextIdleExpiry()
                });
            _aprsListener = new AprsListener(_catalog, areas, worker, tc);

            _aircraftEventHandler = new AircraftEventHandler(_aprsListener, tc);

            _aprsListener.Start();

        }

        /// <summary>UTC hour of the nightly flight-analysis recycle (app setting AprsRecycleHourUtc, default 2).</summary>
        private static int AprsRecycleHourUtc()
        {
            var value = ConfigurationManager.AppSettings["AprsRecycleHourUtc"];
            return int.TryParse(value, out var hour) && hour >= 0 && hour <= 23
                ? hour
                : new FlightAnalysisWorkerOptions().RecycleHourUtc;
        }

        /// <summary>
        /// How long a silent aircraft keeps its flight context (app setting AprsContextIdleExpiryHours, whole hours,
        /// default 6, clamped to 1..24).
        /// </summary>
        private static TimeSpan AprsContextIdleExpiry()
        {
            return FlightAnalysisWorkerOptions.ContextIdleExpiryFromSetting(ConfigurationManager.AppSettings["AprsContextIdleExpiryHours"]);
        }

        /// <summary>
        /// Kill switch for the APRS listener (app setting AprsListenerEnabled, default true).
        /// Changing an App Service app setting restarts the app, so no deploy is needed.
        /// </summary>
        private static bool IsAprsListenerEnabled()
        {
            var value = ConfigurationManager.AppSettings["AprsListenerEnabled"];
            return !bool.TryParse(value, out var enabled) || enabled;
        }
    }
}