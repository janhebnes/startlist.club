using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Boerman.AprsClient;
using Boerman.AprsClient.Enums;
using Boerman.AprsClient.Models;
using Boerman.Core.Spatial;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;
using Skyhop.FlightAnalysis;
using Skyhop.FlightAnalysis.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace FlightJournal.Web.Aprs
{

    public interface IAprsListener : IDisposable
    {
        EventHandler<AircraftEvent> OnAircraftTakeoff { get; set; }
        EventHandler<AircraftEvent> OnAircraftLanding { get; set; }

        void Start();
    }

    public class AircraftEvent
    {
        public Aircraft Aircraft { get; }
        public DateTime? Time { get; }

        public AircraftEvent(Aircraft a, DateTime? t)
        {
            Aircraft = a;
            Time = t?.ToLocalTime();
        }
    }


    public class AprsListener : IAprsListener
    {
        private TelemetryClient _telemetryClient;

        private readonly IAircraftCatalog _catalog;
        private readonly List<Listener> _aprsClients;
        private readonly FlightAnalysisWorker _worker;

        public EventHandler<AircraftEvent> OnAircraftTakeoff { get; set; }
        public EventHandler<AircraftEvent> OnAircraftLanding { get; set; }


        public AprsListener(IAircraftCatalog catalog, IEnumerable<ListenerArea> ranges, FlightAnalysisWorker worker, TelemetryClient telemetryClient)
        {
            _catalog = catalog;
            _worker = worker;

            _worker.OnTakeoff += OnTakeoff;
            _worker.OnLanding += OnLanding;
            _worker.OnRadarContact += OnRadarContact;
            _worker.OnContextDispose += OnContactLost;

            _aprsClients = ranges.Where(r => r.IsValid).Select(CreateListener).ToList();
            _telemetryClient = telemetryClient;
        }

        private Listener CreateListener(ListenerArea r)
        {
            Log.Debug($"Adding aprs listener for area {r}");
            var aprsClient = new Listener(new Config
            {
                Callsign = "0",
                Password = "-1",
                Uri = "aprs.glidernet.org",
                UseOgnAdditives = true,
                Port = 14580, //10152: Full feed, 14580: Filtered
                Filter = $"r/{r.Latitude.ToString(CultureInfo.InvariantCulture)}/{r.Longitude.ToString(CultureInfo.InvariantCulture)}/{r.Radius.ToString(CultureInfo.InvariantCulture)}"
            });

            
            aprsClient.DataReceived += OnAprsDataReceived;
            aprsClient.PacketReceived += OnAprsPacketReceived;
            return aprsClient;
        }

        private void OnAprsPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            switch (e.AprsMessage.DataType)
            {
                case DataType.Status:
                    break;
                case DataType.PositionWithTimestampNoAprsMessaging:
                case DataType.PositionWithTimestampWithAprsMessaging:
                    PositionUpdate positionUpdate;
                    try
                    {
                        if (!e.AprsMessage.IsComplete())
                            break;

                        positionUpdate = new PositionUpdate(
                            e.AprsMessage.Callsign,
                            e.AprsMessage.ReceivedDate.ToUniversalTime(),
                            e.AprsMessage.Latitude.ToDegreesFixed(),
                            e.AprsMessage.Longitude.ToDegreesFixed(),
                            e.AprsMessage.Altitude.MetersAboveSeaLevel,
                            e.AprsMessage.Speed.Knots,
                            e.AprsMessage.Direction.ToDegrees());
                    }
                    catch (Exception ex)
                    {
                        Log.Exception($"{nameof(AprsListener)} OnAprsPacketReceived", ex);
                        break;
                    }

                    // Listener threads only hand off; the worker is the single owner of flight analysis
                    _worker.Post(positionUpdate);
                    break;
                default:
                    break;
            }
        }

        private void OnAprsDataReceived(object sender, AprsDataReceivedEventArgs e)
        {
            //Console.WriteLine($"{DateTime.UtcNow}: {e.Data} - DataReceived");
        }

        private void OnRadarContact(object sender, OnRadarContactEventArgs e)
        {
            // With minified memory pressure the library may already have trimmed the position history
            var lastPositionUpdate = e.Flight.PositionUpdates?.OrderByDescending(q => q.TimeStamp).FirstOrDefault();
            if (lastPositionUpdate == null)
                return;
            var aircraft = _catalog.AircraftInfo((e.Flight.Aircraft));
            Log.Debug($"{nameof(AprsListener)}: {lastPositionUpdate.TimeStamp:o}: {e.Flight.Aircraft} {aircraft.Info()} - Radar contact at ({lastPositionUpdate.Latitude:N4}, {lastPositionUpdate.Longitude:N4}) @ {lastPositionUpdate.Altitude:N0}ft");
        }

        private void OnContactLost(object sender, OnContextDisposedEventArgs e)
        {
            var aircraft = _catalog.AircraftInfo((e.Context.Flight.Aircraft));
            Log.Debug($"{nameof(AprsListener)}: {e.Context.Flight.Aircraft} {aircraft.Info()} - contact lost");
        }

        private void OnTakeoff(object sender, OnTakeoffEventArgs e)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("OnTakeoff"))
            {
                var aircraft = _catalog.AircraftInfo((e.Flight.Aircraft));

                var ae = new AircraftEvent(aircraft, e.Flight.StartTime);
#if DEBUG
                Log.Debug($"{nameof(AprsListener)}: {e.Flight.Aircraft} {ae.Aircraft.Info()} - Took off from ({e.Flight.DepartureLocation.Y:N4},{e.Flight.DepartureLocation.X:N4}) at {ae.Time:o}");
#endif
                _telemetryClient.TrackEvent("AprsTakeoffDetected");
                try
                {
                    OnAircraftTakeoff?.Invoke(this, ae);
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    Log.Exception($"{nameof(AprsListener)} OnTakeoff", ex);
                }
            }
        }

        private void OnLanding(object sender, OnLandingEventArgs e)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("OnLanding"))
            {
                var aircraft = _catalog.AircraftInfo((e.Flight.Aircraft));

                var ae = new AircraftEvent(aircraft, e.Flight.EndTime);
#if DEBUG
                Log.Debug($"{nameof(AprsListener)}: {e.Flight.Aircraft} {ae.Aircraft.Info()} - Landed at ({e.Flight.ArrivalLocation.Y:N4}, {e.Flight.ArrivalLocation.X:N4}) at {ae.Time:o}");
#endif
                _telemetryClient.TrackEvent("AprsLandingDetected");
                try
                {
                    OnAircraftLanding?.Invoke(this, ae);
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    Log.Exception($"{nameof(AprsListener)} OnLanding", ex);
                }
                
            }

        }

        public void Start()
        {
            _worker.Start();
            _aprsClients.ForEach(c => c.Open());
        }

        public void Dispose()
        {
            _aprsClients.ForEach(c => c.Dispose());
            _worker.Dispose();
        }
    }


    public static class LocationExtensions
    {
        public static double ToDegreesFixed(this LatitudeLongitudeBase latlong)
        {
            var h = latlong.Vector;
            var m = h.Minutes / 100.0;

            return (double)h.Degrees + (double)m / 60.0;
        }
    }


    public static class AprsExtensions
    {
        public static bool IsComplete(this AprsMessage message)
        {
            return message.Latitude != null
                   && message.Longitude != null
                   && message.Altitude != null
                   && message.Speed != null
                   && message.Direction != null
                   && message.Callsign != null
                   ;
        }
    }

}