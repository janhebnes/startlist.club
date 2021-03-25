using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace FlightJournal.Web.Models.Training.Catalogue
{
    /// <summary>
    /// A training exercise. The lowest level in the hierarchy. Part of one or more TrainingLessons (defined by Training2LessonExerciseRelation).
    /// Semantics: an exercise can be edited, but if the meaning changes, a new one must be created (as executed/completed exercises refer to this)
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Training2Exercise
    {
        [Key]
        [JsonIgnore]
        public int Training2ExerciseId { get; set; }

        public Guid ExerciseIdForExport { get; set; }

        [Required]
        public string Name { get; set; }

        [AllowHtml]
        public string Note { get; set; } = "";
        
        [AllowHtml]
        public string AcceptanceCriteria { get; set; } = "";

        public bool IsBriefingOnly { get; set; }

        public int DisplayOrder { get; set; }

        public Training2Exercise()
        {
            Lessons = new HashSet<Training2Lesson>();
        }

        [JsonIgnore]
        public virtual ICollection<Training2Lesson> Lessons{ get; set; }
        
        public Training2Exercise(string name, bool briefingOnly = false)
        {
            Name = name;
            IsBriefingOnly = briefingOnly;
            Lessons = new HashSet<Training2Lesson>();
        }
    }
}