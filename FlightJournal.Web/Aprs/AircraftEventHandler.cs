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

        public AircraftEventHandler(IAprsListener aprsListener, TelemetryClient telemetryClient)
        {
            _aprsListener = aprsListener;

            _aprsListener.OnAircraftTakeoff += OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding += OnAircraftLanding;
            _telemetryClient = telemetryClient;
        }


        private void OnAircraftTakeoff(object sender, AircraftEvent e)
        {
            if (e?.Aircraft == null)
                return;

            using (_telemetryClient.StartOperation<RequestTelemetry>("OnAircrafTakeoff"))
            using (var db = new FlightContext())
            {
                var p = FindPlane(db, e.Aircraft);
                if (p == null)
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: starting {e.Aircraft} not in DB - ignored");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: starting {e.Aircraft} not in DB - ignored");
                    return;
                }

                var flights = db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure == null && f.Landing == null).ToList();
                flights = flights.Where(f => f.LastUpdated.Date == DateTime.Today).ToList(); // LINQ to Entities can't do this...

                if (flights.Count() == 1)
                {
                    var flight = flights.Single();
                    Log.Information($"{nameof(AircraftEventHandler)} TAKEOFF: {flight.Plane.Registration} took off at {e.Time:o}");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)} TAKEOFF: {flight.Plane.Registration} took off at {e.Time:o}");
                    if (ShouldUseAutoStartAndLanding(flight, db))
                    {
                        flight.Departure = e.Time ?? DateTime.Now;
                        db.Entry(flight).State = EntityState.Modified;
                        db.SaveChanges();
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
            using (var db = new FlightContext())
            {
                var p = FindPlane(db, e.Aircraft);
                if (p == null)
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: landing {e.Aircraft} not in DB - ignored");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)}: landing {e.Aircraft} not in DB - ignored");
                    return;
                }

                var flights = db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure != null && f.Landing == null).ToList();
                flights = flights.Where(f => f.Departure.HasValue && f.Departure.Value.Date == DateTime.Today).ToList(); // LINQ to Entities can't do this...
                if (flights.Count() == 1)
                {
                    var flight = flights.Single();
                    Log.Information($"{nameof(AircraftEventHandler)} LANDING: {flight.Plane.Registration} landed at {e.Time:o}");
                    _telemetryClient.TrackEvent($"{nameof(AircraftEventHandler)} LANDING: {flight.Plane.Registration} landed at {e.Time:o}");
                    if (ShouldUseAutoStartAndLanding(flight, db))
                    {
                        flight.Landing = e.Time ?? DateTime.Now;
                        db.Entry(flight).State = EntityState.Modified;
                        db.SaveChanges();
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
        /// <param name="db"></param>
        /// <returns></returns>
        private bool ShouldUseAutoStartAndLanding(Flight f, FlightContext db)
        {
            try
            {
                return f.Pilot.Club.UseAPRSTakeoffAndLanding
                       || (f.PilotBackseat?.Club.UseAPRSTakeoffAndLanding ?? false)
                       || db.Clubs.Any(c =>
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
        /// <summary>Targeted lookup; matches on competition id too, due to non-stringent registrations.</summary>
        private static Plane FindPlane(FlightContext db, Aircraft aircraft)
        {
            var registration = aircraft.Registration.ToLower();
            var competitionId = aircraft.CompetitionId.ToLower();
            return db.Planes.FirstOrDefault(x => x.Registration.ToLower() == registration
                                                 || x.CompetitionId.ToLower() == competitionId);
        }

        public void Dispose()
        {
            _aprsListener.OnAircraftTakeoff -= OnAircraftTakeoff;
            _aprsListener.OnAircraftLanding -= OnAircraftLanding;
        }
    }
}
