using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using FlightJournal.Web.Aprs;
using FlightJournal.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Aprs
{
    /// <summary>The allow-list of APRS telemetry properties. Synthetic aircraft, places and times only.</summary>
    [TestClass]
    public class AprsTelemetryTest
    {
        private static readonly string[] AllowedKeys =
        {
            "Event", "DeviceId", "Registration", "CompetitionId", "AircraftType", "ListenerAreaIds", "Location",
            "Latitude", "Longitude", "EventTimeUtc", "ReceivedUtc", "FlightId", "StartType", "Reason", "MatchCount"
        };

        private static AprsTelemetryFacts AllFacts() => new AprsTelemetryFacts
        {
            Event = "Takeoff",
            DeviceId = "FLRTEST01",
            Registration = "TEST-1",
            CompetitionId = "T1",
            AircraftType = "Test glider",
            ListenerAreaIds = "1,2",
            Location = "Test field",
            Latitude = 55.123456,
            Longitude = 9.987654,
            EventTime = new DateTime(2000, 1, 2, 10, 15, 30, DateTimeKind.Utc),
            Received = new DateTime(2000, 1, 2, 10, 15, 31, DateTimeKind.Utc),
            FlightId = new Guid("00000000-0000-0000-0000-000000000001"),
            StartType = "Winch",
            Reason = "NoSingleMatch",
            MatchCount = 2
        };

        [TestMethod]
        public void Properties_AllFacts_ExpectedKeysAndValues()
        {
            var properties = AprsTelemetry.Properties(AllFacts());

            CollectionAssert.AreEquivalent(AllowedKeys, properties.Keys.ToArray());
            Assert.AreEqual("FLRTEST01", properties["DeviceId"]);
            Assert.AreEqual("TEST-1", properties["Registration"]);
            Assert.AreEqual("T1", properties["CompetitionId"]);
            Assert.AreEqual("Test field", properties["Location"]);
            Assert.AreEqual("1,2", properties["ListenerAreaIds"]);
            Assert.AreEqual("2000-01-02T10:15:30Z", properties["EventTimeUtc"]);
            Assert.AreEqual("2000-01-02T10:15:31Z", properties["ReceivedUtc"]);
            Assert.AreEqual("00000000-0000-0000-0000-000000000001", properties["FlightId"]);
            Assert.AreEqual("Winch", properties["StartType"]);
            Assert.AreEqual("NoSingleMatch", properties["Reason"]);
            Assert.AreEqual("2", properties["MatchCount"]);
        }

        [TestMethod]
        public void Properties_Coordinates_RoundedToThreeDecimals_InvariantCulture()
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK"); // decimal comma
                var properties = AprsTelemetry.Properties(new AprsTelemetryFacts { Latitude = 55.123456, Longitude = -9.98761 });

                Assert.AreEqual("55.123", properties["Latitude"]);
                Assert.AreEqual("-9.988", properties["Longitude"]);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        [TestMethod]
        public void Properties_LocalTimes_WrittenAsUtc()
        {
            var local = new DateTime(2000, 6, 1, 12, 0, 0, DateTimeKind.Local);
            var properties = AprsTelemetry.Properties(new AprsTelemetryFacts { EventTime = local });

            Assert.AreEqual(local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), properties["EventTimeUtc"]);
        }

        [TestMethod]
        public void Properties_MissingValues_Omitted()
        {
            var properties = AprsTelemetry.Properties(new AprsTelemetryFacts { Event = "Landing", DeviceId = "FLRTEST02", Registration = "", CompetitionId = "  " });

            CollectionAssert.AreEquivalent(new[] { "Event", "DeviceId" }, properties.Keys.ToArray());
            Assert.IsFalse(properties.Values.Any(v => v == null || v == "null" || v.Trim().Length == 0));
            Assert.AreEqual(0, AprsTelemetry.Properties(new AprsTelemetryFacts()).Count);
            Assert.AreEqual(0, AprsTelemetry.Properties(null).Count);
        }

        [TestMethod]
        public void Facts_CannotCarryPilotOrPersonData()
        {
            // The facts type is the allow-list: there is no field for any person, and none for free text
            var forbidden = new[] { "pilot", "member", "union", "payer", "betaler", "user", "person", "name", "description", "note", "comment", "text" };
            var fields = typeof(AprsTelemetryFacts).GetProperties().Select(p => p.Name.ToLowerInvariant()).ToList();
            var leaks = fields.Where(f => forbidden.Any(f.Contains)).ToList();

            Assert.AreEqual(0, leaks.Count, "Forbidden facts: " + string.Join(", ", leaks));
            Assert.IsTrue(typeof(AprsTelemetry).GetMethods().Where(m => m.Name == nameof(AprsTelemetry.Properties))
                    .SelectMany(m => m.GetParameters()).All(p => p.ParameterType == typeof(AprsTelemetryFacts)),
                "Properties only accepts the allow-listed facts");
            CollectionAssert.IsSubsetOf(AprsTelemetry.Properties(AllFacts()).Keys.ToArray(), AllowedKeys);
        }

        [TestMethod]
        public void ListenerAreaIds_ContainingAreasOnly()
        {
            var areas = new[]
            {
                new ListenerArea { ListenerAreaId = 7, Latitude = 55.0, Longitude = 9.0, Radius = 20 },
                new ListenerArea { ListenerAreaId = 3, Latitude = 55.1, Longitude = 9.1, Radius = 20 },
                new ListenerArea { ListenerAreaId = 9, Latitude = 56.0, Longitude = 10.0, Radius = 20 },
                new ListenerArea { ListenerAreaId = 4, Latitude = 55.0, Longitude = 9.0, Radius = 0 } // invalid
            };

            Assert.AreEqual("3,7", AprsTelemetry.ListenerAreaIds(areas, 55.05, 9.05), "Overlapping areas, sorted");
            Assert.IsNull(AprsTelemetry.ListenerAreaIds(areas, 40.0, 0.0), "Outside every area");
            Assert.IsNull(AprsTelemetry.ListenerAreaIds(areas, null, 9.0));
            Assert.IsNull(AprsTelemetry.ListenerAreaIds(null, 55.0, 9.0));
        }
    }
}
