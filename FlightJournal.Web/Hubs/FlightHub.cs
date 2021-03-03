using Microsoft.AspNet.SignalR;

namespace FlightJournal.Web.Hubs
{
    public class FlightHub : Hub
    {
        public void InformClientsStaleData()
        {
            Clients.All.staleData();
        }
    }
}