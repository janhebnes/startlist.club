using Microsoft.AspNet.SignalR;
using System;

namespace FlightJournal.Web.Hubs
{
    public class FlightHub : Hub
    {
        public void StaleData(Guid flightId)
        {
            var id = flightId;
            Clients.All.DataIsStale();
        }
    }
}