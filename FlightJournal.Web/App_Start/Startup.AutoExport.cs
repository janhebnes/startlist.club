using FlightJournal.Web.AutoExport;
using FlightJournal.Web.Configuration;
using FlightJournal.Web.Extensions;

namespace FlightJournal.Web
{
    public partial class Startup
    {
        private AutoExporter _autoExporter;
        public void ConfigureAutoExport()
        {
            _autoExporter = new AutoExporter(Settings.AutoExports);
            _autoExporter.Start();
        }
    }
}