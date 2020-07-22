using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// A Training2Exercise that has been applied in a flight
    /// </summary>
    public class AppliedExercise
    {
        [Key] 
        public Guid Id { get; set; }

        [Required]
        public Guid FlightId { get; set; } // the pilot can be found through the flight
        [Required]
        public Guid ExerciseId { get; set; }

        [Required] public ExerciseAction Action { get; set; } = ExerciseAction.None;
    }


    public enum ExerciseAction
    {
        None,
        Briefed,
        Trained,
        Completed
    }
}