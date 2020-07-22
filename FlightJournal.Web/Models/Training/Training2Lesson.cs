using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// A training lesson. Example: A7.
    /// Is part of one or more TrainingPrograms (defined by Training2ProgramLessonRelation)
    /// Contains one or more TrainingExercises (defined by Training2LessonExerciseRelation)
    /// </summary>
    public class Training2Lesson
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        //Example: A7
        public string Name { get; set; }

        public string Precondition { get; set; } = "";
        [Required]
        public string Purpose{ get; set; }

        public string AcceptanceCriteria { get; set; } = "";

        public Training2Lesson(){}
        public Training2Lesson(string name, string purpose)
        {
            Id = Guid.NewGuid();
            Name = name;
            Purpose = purpose;
        }
    }
}