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
            var tc = new Microsoft.ApplicationInsights.TelemetryClient();
            var db = new FlightContext();
            Log.Information("Setting up APRS data listener chain");
            _catalog = new AircraftCatalog(null, tc);
            _aprsListener = new AprsListener(_catalog, db.ListenerAreas, tc);

            _aircraftEventHandler = new AircraftEventHandler(_aprsListener, db, tc);

            _aprsListener.Start();
                      
        }
    }
}