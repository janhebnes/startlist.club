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


        private void OnAircraftTakeoff(object sender, AircraftEvent e)
        {
            Plane p = _db.Planes.FirstOrDefault(x => x.Registration.ToLower() == e.Aircraft.Registration.ToLower() 
                                                     || x.CompetitionId.ToLower() == e.Aircraft.CompetitionId.ToLower()); // need comp. id due to non-stringent registrations...
            if (p == null)
            {
                Log.Debug($"{nameof(AircraftEventHandler)}: starting {e.Aircraft} not in DB - ignored");
                return;
            }

            var flights = _db.Flights.Where(f => f.Deleted == null && f.LastUpdated.Date == DateTime.Today && f.Plane.PlaneId == p.PlaneId && f.Departure == null && f.Landing == null);
            if (flights.Count() == 1)
            {
                // we're assuming that the plane takes off from the location specified in the flight.
                // Check if any club at that location is using APRSTakeoffAndLanding

                var flight = flights.Single();
                Log.Information($"{nameof(AircraftEventHandler)} TAKEOFF: {flight.Plane.Registration} took off at {e.Time:o}");
                if (_db.Clubs.Any(c => c.LocationId == flight.StartedFromId && c.UseAPRSTakeoffAndLanding))
                {
                    flight.Departure = e.Time ?? DateTime.Now;
                    _db.Entry(flight).State = EntityState.Modified;
                    _db.SaveChanges();
                    FlightsHub.NotifyFlightStarted(flight.FlightId, Guid.Empty);
                }
                else
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: No clubs at the starting location has APRS autostart enabled - takeoff of {flight.Plane.Registration} ignored");
                }
            }
            else
            {
                Log.Debug($"{nameof(AircraftEventHandler)}: {flights.Count()} pending flights matching {p.Registration} - unable to autostart");
            }

        }

        private void OnAircraftLanding(object sender, AircraftEvent e)
        {
            Plane p = _db.Planes.FirstOrDefault(x => x.Registration.ToLower() == e.Aircraft.Registration.ToLower() 
                                                     || x.CompetitionId.ToLower() == e.Aircraft.CompetitionId.ToLower());
            if (p == null)
            {
                Log.Debug($"{nameof(AircraftEventHandler)}: landing {e.Aircraft} not in DB - ignored");
                return;
            }

            var flights = _db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure!=null && f.Departure.Value.Date == DateTime.Today && f.Landing == null);
            if (flights.Count() == 1)
            {
                var flight = flights.Single();
                Log.Information($"{nameof(AircraftEventHandler)} LANDING: {flight.Plane.Registration} landed at {e.Time:o}");
                if (_db.Clubs.Any(c => c.LocationId == flight.LandedOnId && c.UseAPRSTakeoffAndLanding))
                {
                    flight.Landing = e.Time ?? DateTime.Now;
                    _db.Entry(flight).State = EntityState.Modified; 
                    _db.SaveChanges();
                    FlightsHub.NotifyFlightLanded(flight.FlightId, Guid.Empty);
                }
                else
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: No clubs at the landing location has APRS autostart enabled - landing of {flight.Plane.Registration} ignored");
                }
            }
            else
            {
                Log.Debug($"{nameof(AircraftEventHandler)}: {flights.Count()} airborne flights matching {p.Registration} - unable to autoland");
            }
        }

        public void Dispose()
        {
            _aprsListener.OnAircraftTakeoff -= OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding -= OnAircraftLanding;
        }
    }
}
