using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace FlightJournal.Web.Models.Training.Catalogue
{
    /// <summary>
    /// A training lesson. Example: A7.
    /// Is part of one Training2Programs 
    /// Contains one or more Training2Exercises 
    /// </summary>
    public class Training2Lesson
    {
        [Key]
        public int Training2LessonId { get; set; }
        [Required]
        //Example: A7
        public string Name { get; set; }

        [AllowHtml]
        public string Precondition { get; set; } = "";
        [Required]
        [AllowHtml]
        public string Purpose{ get; set; }

        [AllowHtml]
        public string AcceptanceCriteria { get; set; } = "";

        public virtual ICollection<Training2Program> Programs { get; set; }
        public virtual ICollection<Training2Exercise> Exercises { get; set; }

        public Training2Lesson()
        {
            Programs = new HashSet<Training2Program>();
            Exercises = new HashSet<Training2Exercise>();
        }
        public Training2Lesson(string name, string purpose)
        {
            Name = name;
            Purpose = purpose;
            Programs = new HashSet<Training2Program>();
            Exercises = new HashSet<Training2Exercise>();
        }

    }
}