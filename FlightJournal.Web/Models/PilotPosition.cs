using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Models
{
    /// <summary>
    /// Pilot position during flight PilotInCommand / Student / FlightInstructor / FlightExaminer / Passenger
    /// Funktion som: Luftfartøjschef, Elev, Instruktør, Kontrollant.
    /// </summary>
    public enum PilotPosition
    {
        PilotInCommand,
        FlightInstructor,
        FlightExaminer,
        Student,
        Passenger
    }
}