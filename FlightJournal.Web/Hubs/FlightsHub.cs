using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace FlightJournal.Web.Hubs
{
    public class FlightsHub : Hub<IFlightsHubClient>
    {
        private static readonly IHubContext<IFlightsHubClient> HubContext =
            GlobalHost.ConnectionManager.GetHubContext<FlightsHub, IFlightsHubClient>();

        public static void NotifyFlightChanged(Guid flightId, Guid changeOrigin)
        {
            HubContext.Clients.All.NotifyFlightChanged(flightId, changeOrigin);
        }
        public static void NotifyFlightStarted(Guid flightId, Guid changeOrigin)
        {
            HubContext.Clients.All.NotifyFlightStarted(flightId, changeOrigin);
            HubContext.Clients.All.NotifyFlightChanged(flightId, changeOrigin);
        }
        public static void NotifyFlightLanded(Guid flightId, Guid changeOrigin)
        {
            HubContext.Clients.All.NotifyFlightLanded(flightId, changeOrigin);
            HubContext.Clients.All.NotifyFlightChanged(flightId, changeOrigin);
        }
        public static void NotifyFlightAdded(Guid flightId, Guid changeOrigin, IEnumerable<int> affectedLocationIds)
        {
            HubContext.Clients.All.NotifyFlightAdded(flightId, changeOrigin, affectedLocationIds);
        }
        public static void NotifyTrainingDataChanged(Guid flightId, Guid changeOrigin)
        {
            HubContext.Clients.All.NotifyTrainingDataChanged(flightId, changeOrigin);
        }
    }

    public interface IFlightsHubClient
    {
        void NotifyFlightChanged(Guid flightId, Guid changeOrigin);
        void NotifyFlightStarted(Guid flightId, Guid changeOrigin);
        void NotifyFlightLanded(Guid flightId, Guid changeOrigin);
        void NotifyFlightAdded(Guid flightId, Guid changeOrigin, IEnumerable<int> affectedLocationIds);
        void NotifyTrainingDataChanged(Guid flightId, Guid changeOrigin);
    }
}