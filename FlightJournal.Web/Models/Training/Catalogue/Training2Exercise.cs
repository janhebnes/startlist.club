using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training.Catalogue
{
    /// <summary>
    /// A training exercise. The lowest level in the hierarchy. Part of one or more TrainingLessons (defined by Training2LessonExerciseRelation).
    /// Semantics: an exercise can be edited, but if the meaning changes, a new one must be created (as executed/completed exercises refer to this)
    /// 
    /// </summary>
    public class Training2Exercise
    {
        [Key]
        public int Training2ExerciseId { get; set; }
        [Required]
        // Example: "Mærkelanding"
        public string Name { get; set; }

        public string Note { get; set; } = "";
        // Example: "Final position must deviate max 5m sideways and 10m longitudinally from the T"
        public string AcceptanceCriteria { get; set; } = "";

        // 
        public bool IsBriefingOnly { get; set; }

        public Training2Exercise() { }
        public virtual ICollection<Training2Lesson> Lessons{ get; set; }

        public Training2Exercise(string name, bool briefingOnly = false)
        {
            Name = name;
            IsBriefingOnly = briefingOnly;
            Lessons = new HashSet<Training2Lesson>();
        }
    }
}