using System;
using FlightJournal.Web.FlightExport;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;

namespace FlightJournal.Web
{
    public partial class Startup
    {

        public void ConfigureAutoExport()
        {
            var db = new FlightContext();
            Log.Information("Setting up Automatic export");

            TimeSpan interval;
            switch (ApplicationConfiguration.Config)
            {
                case AppConfig.CFG_RELEASE:
                    interval = TimeSpan.FromMinutes(60);
                    break;
                case AppConfig.CFG_DEMO:
                case AppConfig.CFG_DEV:
                    interval = TimeSpan.Zero;
                    break;
                default:
                    interval = TimeSpan.FromMinutes(1);
                    break;
            }
            var exporter = new AutoExporter(db, interval);
        }

        
    }
}