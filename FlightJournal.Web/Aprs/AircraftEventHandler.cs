using System;
using System.Data.Entity;
using System.Linq;
using FlightJournal.Web.Hubs;
using FlightJournal.Web.Logging;
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
            if (p == null)
            {
                Log.Debug($"{nameof(AircraftEventHandler)}: starting {aircraft} not in DB - ignored");
                return;
            }

            var flights = _db.Flights.Where(f => f.Plane.PlaneId == p.PlaneId && f.Departure == null && f.Landing == null);
            if (flights.Count() == 1)
            {
                var flight = flights.Single();
                Log.Information($"{nameof(AircraftEventHandler)}: {flight.Plane.Registration} taking off");
                flight.Departure = DateTime.Now;
                _db.Entry(flight).State = EntityState.Modified;
                _db.SaveChanges();
                FlightsHub.NotifyFlightStarted(flight.FlightId, Guid.Empty);
            }
            else
            {
                Log.Information($"{nameof(AircraftEventHandler)}: {flights.Count()} pending flights matching {p.Registration} - unable to autostart");
            }

        }

        private void OnAircraftLanding(object sender, Aircraft aircraft)
        {
            Plane p = _db.Planes.FirstOrDefault(x => x.Registration.ToLower() == aircraft.Registration.ToLower());
            if (p == null)
            {
                Log.Debug($"{nameof(AircraftEventHandler)}: landing {aircraft} not in DB - ignored");
                return;
            }

            var flights = _db.Flights.Where(f => f.Plane.PlaneId == p.PlaneId && f.Departure != null && f.Landing == null);
            if (flights.Count() == 1)
            {
                var flight = flights.Single();
                Log.Information($"{nameof(AircraftEventHandler)}: {flight.Plane.Registration} landing");
                flight.Landing = DateTime.Now;
                _db.Entry(flight).State = EntityState.Modified;
                _db.SaveChanges();
                FlightsHub.NotifyFlightLanded(flight.FlightId, Guid.Empty);
            }
            else
            {
                Log.Warning($"{nameof(AircraftEventHandler)}: {flights.Count()} airborne flights matching {p.Registration} - unable to autoland");
            }
        }

        public void Dispose()
        {
            _aprsListener.OnAircraftTakeoff -= OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding -= OnAircraftLanding;
        }
    }
}