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
            var db = new FlightContext();
            Log.Information("Setting up APRS data listener chain");
            _catalog = new AircraftCatalog(null, tc);
            _aprsListener = new AprsListener(_catalog, db.ListenerAreas, tc);

            _aircraftEventHandler = new AircraftEventHandler(_aprsListener, db, tc);

            _aprsListener.Start();

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