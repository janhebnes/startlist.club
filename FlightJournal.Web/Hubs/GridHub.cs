using Microsoft.AspNet.SignalR;

namespace FlightJournal.Web.Hubs
{
    public class GridHub : Hub
    {
        public void NewFlight()
        {
            Clients.All.newFlightCreated();
        }
    }
}