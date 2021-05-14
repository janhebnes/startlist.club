using System;
using System.Linq;
using Boerman.AprsClient;
using Boerman.AprsClient.Enums;
using Boerman.AprsClient.Models;
using Boerman.Core.Spatial;
using Skyhop.FlightAnalysis;
using Skyhop.FlightAnalysis.Models;

namespace FlightJournal.Web.Aprs
{
    public interface IAprsListener : IDisposable
    {
        EventHandler<Aircraft> OnAircraftTakeoff { get; set; }
        EventHandler<Aircraft> OnAircraftLanding { get; set; }

        void Start();
    }

    public class AprsListener : IAprsListener
    {
        private readonly IAircraftCatalog _catalog;
        private static Listener _aprsClient;
        private static readonly FlightContextFactory FlightContextFactory = new FlightContextFactory();

        public EventHandler<Aircraft> OnAircraftTakeoff { get; set; }
        public EventHandler<Aircraft> OnAircraftLanding { get; set; }


        public AprsListener(IAircraftCatalog catalog)
        {
            _catalog = catalog;


            FlightContextFactory.OnTakeoff += OnTakeoff;
            FlightContextFactory.OnLanding += OnLanding;
            FlightContextFactory.OnRadarContact += OnRadarContact;
            FlightContextFactory.OnContextDispose += OnContactLost;

            _aprsClient = new Listener(new Config
            {
                Callsign = "0",
                Password = "-1",
                Uri = "aprs.glidernet.org",
                UseOgnAdditives = true,
                Port = 14580, //10152: Full feed, 14580: Filtered
                Filter = "r/55.84/10.63/220" //DK center 220km radius - multiple filters seem not to work ?
            });

            _aprsClient.DataReceived += OnAprsDataReceived;
            _aprsClient.PacketReceived += OnAprsPacketReceived;
        }

        private void OnAprsPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            switch (e.AprsMessage.DataType)
            {
                case DataType.Status:
                    //                        Console.WriteLine(e.AprsMessage);
                    break;
                case DataType.PositionWithTimestampNoAprsMessaging:
                case DataType.PositionWithTimestampWithAprsMessaging:
                    //                        Console.WriteLine(e.AprsMessage);
                    try
                    {
                        var positionUpdate = new Skyhop.FlightAnalysis.Models.PositionUpdate(
                            e.AprsMessage.Callsign,
                            e.AprsMessage.ReceivedDate,
                            e.AprsMessage.Latitude.ToDegreesFixed(),
                            e.AprsMessage.Longitude.ToDegreesFixed(),
                            e.AprsMessage.Altitude.FeetAboveSeaLevel,
                            e.AprsMessage.Speed.Knots,
                            e.AprsMessage.Direction.ToDegrees());

                        FlightContextFactory.Enqueue(positionUpdate);
                    }
                    catch (NullReferenceException ex)
                    {
                        //                    Console.WriteLine(nullReferenceException.Message);
                        //throw;
                    }
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
            var lastPositionUpdate = e.Flight.PositionUpdates.OrderByDescending(q => q.TimeStamp).First();
            var aircraft = _catalog.AircraftInfo((e.Flight.Aircraft));

            Console.WriteLine($"{lastPositionUpdate.TimeStamp}: {e.Flight.Aircraft} {aircraft.Info()} - Radar contact at ({lastPositionUpdate.Latitude:N4}, {lastPositionUpdate.Longitude:N4}) @ {lastPositionUpdate.Altitude:N0}ft");
        }
        private void OnContactLost(object sender, OnContextDisposedEventArgs e)
        {
            var aircraft = _catalog.AircraftInfo((e.Context.Flight.Aircraft));
            Console.WriteLine($"{DateTime.UtcNow}: {e.Context.Flight.Aircraft} {aircraft.Info()} - contact lost");
        }

        private void OnTakeoff(object sender, OnTakeoffEventArgs e)
        {
            var aircraft = _catalog.AircraftInfo((e.Flight.Aircraft));

            Console.WriteLine($"{DateTime.UtcNow}: {e.Flight.Aircraft} {aircraft.Info()} - Took off from ({e.Flight.DepartureLocation.Y:N4},{e.Flight.DepartureLocation.X:N4}) at {e.Flight.LastSeen}");
        }

        private void OnLanding(object sender, OnLandingEventArgs e)
        {
            var aircraft = _catalog.AircraftInfo((e.Flight.Aircraft));

            Console.WriteLine($"{DateTime.UtcNow}: {e.Flight.Aircraft} {aircraft.Info()} - Landed at ({e.Flight.ArrivalLocation.Y:N4}, {e.Flight.ArrivalLocation.X:N4}) at {e.Flight.LastSeen}");
        }

        public void Start()
        {
            _aprsClient.Open();
        }

        public void Dispose()
        {
            _aprsClient.Dispose();
        }
    }


    public static class LocationExtensions
    {
        public static double ToDegreesFixed(this LatitudeLongitudeBase latlong)
        {
            var h = latlong.Vector;
            if (h.Minutes > 60)
            {
                var m = h.Minutes / 100;

                return (double)h.Degrees + (double)m / 60.0;

            }
            else
                return h.ToDegrees();
        }
    }

}