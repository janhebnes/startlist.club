using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Aprs
{
    /// <summary>
    /// The facts an APRS telemetry event may carry: the aircraft's identity, where and when it was seen, and the outcome.
    /// This type is the allow-list. It deliberately has no field for a pilot, payer, member or user, and none for free
    /// text, so person data can't reach APRS telemetry. Aircraft identity is logged regardless of the owner's OGN
    /// identification preference; telemetry is internal.
    /// </summary>
    public sealed class AprsTelemetryFacts
    {
        /// <summary>Takeoff or Landing.</summary>
        public string Event { get; set; }

        /// <summary>The tracker (OGN device) id the detection came from.</summary>
        public string DeviceId { get; set; }

        public string Registration { get; set; }
        public string CompetitionId { get; set; }
        public string AircraftType { get; set; }

        /// <summary>Ids of the listener areas containing the position, comma separated.</summary>
        public string ListenerAreaIds { get; set; }

        /// <summary>The resolved location (airfield) of a matched flight.</summary>
        public string Location { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>When the takeoff or landing happened, per the APRS data.</summary>
        public DateTime? EventTime { get; set; }

        /// <summary>When the application handled the detection.</summary>
        public DateTime? Received { get; set; }

        /// <summary>The flight that was updated.</summary>
        public Guid? FlightId { get; set; }

        public string StartType { get; set; }

        /// <summary>Why a detection didn't update a flight.</summary>
        public string Reason { get; set; }

        public int? MatchCount { get; set; }

        public AprsTelemetryFacts Copy() => (AprsTelemetryFacts)MemberwiseClone();
    }

    public static class AprsTelemetry
    {
        /// <summary>
        /// The custom properties for an APRS telemetry event: missing values are omitted, coordinates are rounded to
        /// 3 decimals and times are UTC ISO 8601, all culture invariant.
        /// </summary>
        public static Dictionary<string, string> Properties(AprsTelemetryFacts facts)
        {
            var properties = new Dictionary<string, string>();
            if (facts == null)
                return properties;

            Add(properties, "Event", facts.Event);
            Add(properties, "DeviceId", facts.DeviceId);
            Add(properties, "Registration", facts.Registration);
            Add(properties, "CompetitionId", facts.CompetitionId);
            Add(properties, "AircraftType", facts.AircraftType);
            Add(properties, "ListenerAreaIds", facts.ListenerAreaIds);
            Add(properties, "Location", facts.Location);
            Add(properties, "Latitude", Coordinate(facts.Latitude));
            Add(properties, "Longitude", Coordinate(facts.Longitude));
            Add(properties, "EventTimeUtc", Utc(facts.EventTime));
            Add(properties, "ReceivedUtc", Utc(facts.Received));
            Add(properties, "FlightId", facts.FlightId?.ToString());
            Add(properties, "StartType", facts.StartType);
            Add(properties, "Reason", facts.Reason);
            Add(properties, "MatchCount", facts.MatchCount?.ToString(CultureInfo.InvariantCulture));
            return properties;
        }

        /// <summary>The ids of the valid listener areas whose radius (km) contains the position, sorted, or null.</summary>
        public static string ListenerAreaIds(IEnumerable<ListenerArea> areas, double? latitude, double? longitude)
        {
            if (areas == null || !latitude.HasValue || !longitude.HasValue)
                return null;

            var ids = areas
                .Where(a => a.IsValid && DistanceKm(a.Latitude, a.Longitude, latitude.Value, longitude.Value) <= a.Radius)
                .Select(a => a.ListenerAreaId)
                .OrderBy(id => id)
                .ToList();
            return ids.Count == 0 ? null : string.Join(",", ids);
        }

        private static void Add(Dictionary<string, string> properties, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                properties[key] = value.Trim();
        }

        private static string Coordinate(double? value) =>
            value.HasValue ? Math.Round(value.Value, 3, MidpointRounding.AwayFromZero).ToString("0.000", CultureInfo.InvariantCulture) : null;

        private static string Utc(DateTime? value)
        {
            if (!value.HasValue)
                return null;
            var utc = value.Value.Kind == DateTimeKind.Local ? value.Value.ToUniversalTime() : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
            return utc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
