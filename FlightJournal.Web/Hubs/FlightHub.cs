using Microsoft.AspNet.SignalR;
using System;

namespace FlightJournal.Web.Hubs
{
    public class FlightHub : Hub
    {
        public void StaleData(Guid flightId, Guid sessionId)
        {
            Clients.All.dataIsStale(flightId, sessionId);
        }
    }
}