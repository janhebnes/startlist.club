using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// Annotations for a training flight that do not directly refer to a Training2Exercise.
    /// Textual not (by instructor)
    /// SFIL classification./// 
    /// Wind (direction, strength)
    /// 
    /// </summary>
    public class TrainingFlightAnnotation
    {
        [Key]
        public int TrainingFlightAnnotationId { get; set; }
        [Required]
        public Guid FlightId{ get; set; } 
        public string Note { get; set; }

        public IEnumerable<FlightPhaseAnnotation> StartAnnotation { get; set; } = new List<FlightPhaseAnnotation>();
        public IEnumerable<FlightPhaseAnnotation> FlightAnnotation { get; set; } = new List<FlightPhaseAnnotation>(); // applies to the flight in general, as well as the maneuvers
        public IEnumerable<FlightPhaseAnnotation> ApproachAnnotation { get; set; } = new List<FlightPhaseAnnotation>();
        public IEnumerable<FlightPhaseAnnotation> LandingAnnotation { get; set; } = new List<FlightPhaseAnnotation>();

        public IEnumerable<FlightManeuver> Maneuvers { get; set; } = new List<FlightManeuver>();
        public int WindDirection { get; set; } = -1;
        public int WindSpeedInKnots { get; set; } = -1;
    }

    public enum FlightPhaseAnnotation
    {
        Ok,
        AlmostOk,
        InstructorGuidanceNeeded,
        InstructorTakeoverNeeded,
        UnstableDirection,
        UnstableSpeed,
        SpeedTooHigh,
        SpeedTooLow,
        UnstablePosition,
        PositionTooHigh,
        PositionTooLow,
        FlareTooHigh,
        FlareTooLow,
        Skull
    }

    public enum FlightManeuver
    {
        Left360,
        Right360,
        Left180,
        Right180,
        Left90,
        Right90,
        FigureEight,
        STurn,
        Bank30,
        Bank45,
        Bank60,
        AbortedStartLowAltitude,
        AbortedStartMediumAltitude,
        AbortedStartHighAltitude,
        LeftCircuit,
        RightCircuit
    }
}