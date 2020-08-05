using System;
using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Models.Training.Flight
{
    /// <summary>
    /// A Training2Exercise (belonging to a specific lesson in a program) that has been applied in a flight
    /// </summary>
    public class AppliedExercise
    {
        [Key] 
        public int AppliedExerciseId { get; set; }

        [Required]
        public Guid FlightId { get; set; } // the pilot can be found through the flight
        [Required]
        public virtual Training2Program Program { get; set; }
        [Required]
        public virtual Training2Lesson Lesson { get; set; }
        [Required]
        public virtual Training2Exercise Exercise { get; set; }

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