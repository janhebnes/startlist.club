using System;
using System.Collections.Generic;
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
                    TrackIgnored(e, Takeoff, "UnknownAircraft");
                    return;
                }

                var flights = db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure == null && f.Landing == null).ToList();
                flights = flights.Where(f => f.LastUpdated.Date == DateTime.Today).ToList(); // LINQ to Entities can't do this...

                if (flights.Count() == 1)
                {
                    var flight = flights.Single();
                    Log.Debug($"{nameof(AircraftEventHandler)} TAKEOFF: {flight.Plane.Registration} took off at {e.Time:o}");
                    if (ShouldUseAutoStartAndLanding(flight, db))
                    {
                        flight.Departure = e.Time ?? DateTime.Now;
                        db.Entry(flight).State = EntityState.Modified;
                        db.SaveChanges();
                        Log.Information($"{nameof(AircraftEventHandler)}: takeoff registered");
                        TrackRegistered(e, Takeoff, flight);
                        FlightsHub.NotifyFlightStarted(flight.FlightId, Guid.Empty);
                    }
                    else
                    {
                        Log.Debug($"{nameof(AircraftEventHandler)}: APRS autostart not enabled - takeoff of {flight.Plane.Registration} ignored");
                        TrackIgnored(e, Takeoff, "AutoStartNotEnabled", flight: flight);
                    }
                }
                else
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: {flights.Count()} pending flights matching {p.Registration} - unable to autostart");
                    TrackIgnored(e, Takeoff, "NoSingleMatch", flights.Count());
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
                    TrackIgnored(e, Landing, "UnknownAircraft");
                    return;
                }

                var flights = db.Flights.Where(f => f.Deleted == null && f.Plane.PlaneId == p.PlaneId && f.Departure != null && f.Landing == null).ToList();
                flights = flights.Where(f => f.Departure.HasValue && f.Departure.Value.Date == DateTime.Today).ToList(); // LINQ to Entities can't do this...
                if (flights.Count() == 1)
                {
                    var flight = flights.Single();
                    Log.Debug($"{nameof(AircraftEventHandler)} LANDING: {flight.Plane.Registration} landed at {e.Time:o}");
                    if (ShouldUseAutoStartAndLanding(flight, db))
                    {
                        flight.Landing = e.Time ?? DateTime.Now;
                        db.Entry(flight).State = EntityState.Modified;
                        db.SaveChanges();
                        Log.Information($"{nameof(AircraftEventHandler)}: landing registered");
                        TrackRegistered(e, Landing, flight);
                        FlightsHub.NotifyFlightLanded(flight.FlightId, Guid.Empty);
                    }
                    else
                    {
                        Log.Debug($"{nameof(AircraftEventHandler)}: APRS autolanding not enabled - landing of {flight.Plane.Registration} ignored");
                        TrackIgnored(e, Landing, "AutoLandingNotEnabled", flight: flight);
                    }
                }
                else
                {
                    Log.Debug($"{nameof(AircraftEventHandler)}: {flights.Count()} airborne flights matching {p.Registration} - unable to autoland");
                    TrackIgnored(e, Landing, "NoSingleMatch", flights.Count());
                }
            }
        }

        private const string Takeoff = "Takeoff";
        private const string Landing = "Landing";

        /// <summary>
        /// A detection that updated a flight. The event name is constant; the allow-listed properties (AprsTelemetry)
        /// add the aircraft, position, times, the flight id, its location and start type, never the pilot or payer.
        /// </summary>
        private void TrackRegistered(AircraftEvent e, string detection, Flight flight)
        {
            var facts = OutcomeFacts(e, detection, flight);
            facts.FlightId = flight.FlightId;
            _telemetryClient.TrackEvent(detection == Takeoff ? "AprsTakeoffRegistered" : "AprsLandingRegistered", AprsTelemetry.Properties(facts));
        }

        /// <summary>A detection that didn't update a flight, with the reason and, for an ambiguous match, the match count.</summary>
        private void TrackIgnored(AircraftEvent e, string detection, string reason, int? matchCount = null, Flight flight = null)
        {
            var facts = OutcomeFacts(e, detection, flight);
            facts.Reason = reason;
            facts.MatchCount = matchCount;
            _telemetryClient.TrackEvent("AprsEventIgnored", AprsTelemetry.Properties(facts));
        }

        private static AprsTelemetryFacts OutcomeFacts(AircraftEvent e, string detection, Flight flight)
        {
            var facts = e.Facts.Copy();
            facts.Event = detection;
            facts.EventTime = facts.EventTime ?? e.Time;
            if (flight != null)
            {
                var location = detection == Takeoff ? flight.StartedFrom : flight.LandedOn ?? flight.StartedFrom;
                facts.Location = location?.Name;
                facts.StartType = flight.StartType?.Name;
            }
            return facts;
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
