using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// Defines a relation between a Training2Program and a Training2Lesson (N:M)
    /// </summary>
    public class Training2ProgramLessonRelation
    {
        public int RelativeOrder { get; set;  }

        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProgramId { get; set; }
        [Required]
        public Guid LessonId { get; set; }
        public Training2ProgramLessonRelation(){}
        public Training2ProgramLessonRelation(Training2Program program, Training2Lesson lesson, int relativeOrder)
        {
            Id = Guid.NewGuid();
            ProgramId = program.Id;
            LessonId = lesson.Id;
            RelativeOrder = relativeOrder;
        }
    }
}