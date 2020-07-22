using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// Defines a relation between a Training2Lesson and a Training2Exercise (N:M)
    /// </summary>
    public class Training2LessonExerciseRelation
    {
        public int RelativeOrder { get; set; }

        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid LessonId { get; set; }
        [Required]
        public Guid ExerciseId { get; set; }

        public Training2LessonExerciseRelation() { }

        public Training2LessonExerciseRelation(Training2Lesson lesson, Training2Exercise exercise, int relativeOrder)
        {
            RelativeOrder = relativeOrder;
            Id = Guid.NewGuid();
            ExerciseId = exercise.Id;
            LessonId = lesson.Id;
            RelativeOrder = relativeOrder;
        }

    }
}