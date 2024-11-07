using System;
using System.Data.Entity;
using System.Linq;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Hubs;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace FlightJournal.Web.Aprs
{
    public class AircraftEventHandler : IDisposable
    {
        private TelemetryClient _telemetryClient;

        private readonly IAprsListener _aprsListener;
        private readonly FlightContext _db;

        public AircraftEventHandler(IAprsListener aprsListener, FlightContext db, TelemetryClient telemetryClient)
        {
            _aprsListener = aprsListener;
            _db = db;

            _aprsListener.OnAircraftTakeoff += OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding += OnAircraftLanding;
            _telemetryClient = telemetryClient;
        }


        private void OnAircraftTakeoff(object sender, AircraftEvent e)
        {
            if (e?.Aircraft == null)
                return;

            using (_telemetryClient.StartOperation<RequestTelemetry>("OnAircrafTakeoff"))
            {
                var planes = _db.Planes.ToList();
                var p = planes.FirstOrDefault(x => x.Registration.ToLower() == e.Aircraft.Registration.ToLower()
                                                         || x.CompetitionId.ToLower() == e.Aircraft.CompetitionId.ToLower()); // need comp. id due to non-stringent registrations...
                if (p == null)
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: starting {e.Aircraft} not in DB - ignored");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: starting {e.Aircraft} not in DB - ignored");
                    return;
                }

                var flights = _db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure == null && f.Landing == null).ToList();
                flights = flights.Where(f => f.LastUpdated.Date == DateTime.Today).ToList(); // LINQ to Entities can't do this...

                if (flights.Count() == 1)
                {
                    var flight = flights.Single();
                    Log.Information($"{nameof(AircraftEventHandler)} TAKEOFF: {flight.Plane.Registration} took off at {e.Time:o}");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)} TAKEOFF: {flight.Plane.Registration} took off at {e.Time:o}");
                    if (ShouldUseAutoStartAndLanding(flight))
                    {
                        flight.Departure = e.Time ?? DateTime.Now;
                        _db.Entry(flight).State = EntityState.Modified;
                        _db.SaveChanges();
                        FlightsHub.NotifyFlightStarted(flight.FlightId, Guid.Empty);
                    }
                    else
                    {
                        Log.Debug($"{nameof(AircraftEventHandler)}: APRS autostart not enabled - landing of {flight.Plane.Registration} ignored");
                        _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: APRS autostart not enabled - landing of {flight.Plane.Registration} ignored");
                    }
                }
                else
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: {flights.Count()} pending flights matching {p.Registration} - unable to autostart");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: {flights.Count()} pending flights matching {p.Registration} - unable to autostart");
                }
            }
        }

        private void OnAircraftLanding(object sender, AircraftEvent e)
        {
            if (e?.Aircraft == null)
                return;

            using (_telemetryClient.StartOperation<RequestTelemetry>("OnAircraftLanding"))
            {
                var planes = _db.Planes.ToList();
                var p = planes.FirstOrDefault(x => x.Registration.ToLower() == e.Aircraft.Registration.ToLower()
                                                         || x.CompetitionId.ToLower() == e.Aircraft.CompetitionId.ToLower());
                if (p == null)
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: landing {e.Aircraft} not in DB - ignored");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: landing {e.Aircraft} not in DB - ignored");
                    return;
                }

                var flights = _db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure != null && f.Landing == null).ToList();
                flights = flights.Where(f => f.Departure.HasValue && f.Departure.Value.Date == DateTime.Today).ToList(); // LINQ to Entities can't do this...
                if (flights.Count() == 1)
                {
                    var flight = flights.Single();
                    Log.Information($"{nameof(AircraftEventHandler)} LANDING: {flight.Plane.Registration} landed at {e.Time:o}");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)} LANDING: {flight.Plane.Registration} landed at {e.Time:o}");
                    if (ShouldUseAutoStartAndLanding(flight))
                    {
                        flight.Landing = e.Time ?? DateTime.Now;
                        _db.Entry(flight).State = EntityState.Modified;
                        _db.SaveChanges();
                        FlightsHub.NotifyFlightLanded(flight.FlightId, Guid.Empty);
                    }
                    else
                    {
                        Log.Debug($"{nameof(AircraftEventHandler)}: APRS autolanding not enabled - landing of {flight.Plane.Registration} ignored");
                        _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: APRS autolanding not enabled - landing of {flight.Plane.Registration} ignored");
                    }
                }
                else
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: {flights.Count()} airborne flights matching {p.Registration} - unable to autoland");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: {flights.Count()} airborne flights matching {p.Registration} - unable to autoland");
                }
            }
        }

        /// <summary>
        /// Check if the flight should be autostarted and autolanded.
        ///
        /// This is the case if any of the pilots are in a club using this, or if any club on the location is using this.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private bool ShouldUseAutoStartAndLanding(Flight f)
        {
            try
            {
                return f.Pilot.Club.UseAPRSTakeoffAndLanding
                       || (f.PilotBackseat?.Club.UseAPRSTakeoffAndLanding ?? false)
                       || _db.Clubs.Any(c =>
                           (c.LocationId == f.LandedOnId || c.LocationId == f.StartedFromId) 
                           && c.UseAPRSTakeoffAndLanding)
                    ;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return false;
            }
        }
        public void Dispose()
        {
            _aprsListener.OnAircraftTakeoff -= OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding -= OnAircraftLanding;
        }
    }
}
