using System;
using System.Data.Entity;
using System.Linq;
using FlightJournal.Web.Hubs;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Aprs
{
    public class AircraftEventHandler : IDisposable
    {
        private readonly IAprsListener _aprsListener;
        private readonly FlightContext _db;

        public AircraftEventHandler(IAprsListener aprsListener, FlightContext db)
        {
            _aprsListener = aprsListener;
            _db = db;

            _aprsListener.OnAircraftTakeoff += OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding += OnAircraftLanding;
        }


        private void OnAircraftTakeoff(object sender, Aircraft aircraft)
        {
            Plane p = _db.Planes.FirstOrDefault(x => x.Registration.ToLower() == aircraft.Registration.ToLower());
            if (p == null) return;

            var flights = _db.Flights.Where(f => f.Plane == p && f.Departure == null && f.Landing == null);
            if (flights.Count() == 1)
            {
                var flight = flights.Single();
                flight.Landing = DateTime.Now;
                _db.Entry(flight).State = EntityState.Modified;
                _db.SaveChanges();
                FlightsHub.NotifyFlightLanded(flight.FlightId, Guid.Empty);
            }
            else
            {
                // log error - multiple pending flights for this plane, we cannot autopick one
            }

        }

        private void OnAircraftLanding(object sender, Aircraft aircraft)
        {
            Plane p = _db.Planes.FirstOrDefault(x => x.Registration.ToLower() == aircraft.Registration.ToLower());
            if (p == null) return;

            var flights = _db.Flights.Where(f => f.Plane == p && f.Departure != null && f.Landing == null);
            if (flights.Count() == 1)
            {
                var flight = flights.Single();
                flight.Landing = DateTime.Now;
                _db.Entry(flight).State = EntityState.Modified;
                _db.SaveChanges();
                FlightsHub.NotifyFlightLanded(flight.FlightId, Guid.Empty);
            }
            else
            {
                // log error - only one flight with this plane should be flying
            }
        }

        public void Dispose()
        {
            _aprsListener.OnAircraftTakeoff -= OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding -= OnAircraftLanding;
        }
    }
}