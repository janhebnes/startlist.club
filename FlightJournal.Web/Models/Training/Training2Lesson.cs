using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// A training lesson. Example: A7.
    /// Is part of one or more Training2Programs 
    /// Contains one or more Training2Exercises 
    /// </summary>
    public class Training2Lesson
    {
        [Key]
        public int Training2LessonId { get; set; }
        [Required]
        //Example: A7
        public string Name { get; set; }

        public string Precondition { get; set; } = "";
        [Required]
        public string Purpose{ get; set; }

        public string AcceptanceCriteria { get; set; } = "";

        public virtual ICollection<Training2Program> Programs { get; set; }
        public virtual ICollection<Training2Exercise> Exercises { get; set; }
        public Training2Lesson(){}
        public Training2Lesson(string name, string purpose)
        {
            Name = name;
            Purpose = purpose;
            Programs = new HashSet<Training2Program>();
            Exercises = new HashSet<Training2Exercise>();
        }

    }
}