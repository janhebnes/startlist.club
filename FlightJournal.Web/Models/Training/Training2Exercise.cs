using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// A training exercise. The lowest level in the hierarchy. Part of one or more TrainingLessons (defined by Training2LessonExerciseRelation).
    /// Semantics: an exercise can be edited, but if the meaning changes, a new one must be created (as executed/completed exercises refer to this)
    /// 
    /// </summary>
    public class Training2Exercise
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        // Example: "Mærkelanding"
        public string Name { get; set; }

        public string Note { get; set; } = "";
        // Example: "Final position must deviate max 5m sideways and 10m longitudinally from the T"
        public string AcceptanceCriteria { get; set; } = "";

        // 
        public bool IsBriefingOnly { get; set; }

        public Training2Exercise() { }

        public Training2Exercise(string name, bool briefingOnly = false)
        {
            Id = Guid.NewGuid();
            Name = name;
            IsBriefingOnly = briefingOnly;
        }
    }
}