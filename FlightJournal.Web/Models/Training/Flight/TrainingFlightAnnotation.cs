using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models.Training.Flight
{
    /// <summary>
    /// Annotations for a training flight that do not directly refer to a Training2Exercise.
    /// Textual note (by instructor)
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

        public IEnumerable<Commentary> StartAnnotation { get; set; } = new List<Commentary>();
        public IEnumerable<Commentary> FlightAnnotation { get; set; } = new List<Commentary>(); // applies to the flight in general, as well as the maneuvers
        public IEnumerable<Commentary> ApproachAnnotation { get; set; } = new List<Commentary>();
        public IEnumerable<Commentary> LandingAnnotation { get; set; } = new List<Commentary>();

        public IEnumerable<FlightManeuver> Maneuvers { get; set; } = new List<FlightManeuver>();
        public IEnumerable<Predefined.Manouvre> Manouvres { get; set; } = new List<Predefined.Manouvre>();
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