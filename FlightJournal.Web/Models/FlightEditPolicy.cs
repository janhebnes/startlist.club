using System;

namespace FlightJournal.Web.Models
{
    /// <summary>Single source of truth for who may modify a flight.</summary>
    public static class FlightEditPolicy
    {
        public const string DeniedMessage =
            "Flights can only be changed for a limited time after the flight date unless you are an editor.";

        /// <summary>Pilot edit window. Preserves the historic Date.AddDays(3) >= now semantics.</summary>
        public static bool IsWithinPilotEditWindow(DateTime flightDate, DateTime now)
        {
            return flightDate.AddDays(3) >= now;
        }

        public static bool CanEdit(DateTime storedFlightDate, bool isEditor, DateTime now)
        {
            return isEditor || IsWithinPilotEditWindow(storedFlightDate, now);
        }
    }
}
