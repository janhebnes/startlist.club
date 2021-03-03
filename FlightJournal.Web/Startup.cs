using Owin;

namespace FlightJournal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureOpenGliderNetworkFlightLogMonitor(app);
            app.MapSignalR();
        }
    }
}
