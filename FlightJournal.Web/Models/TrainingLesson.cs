using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class TrainingLesson
    {
        [Key]
        public int TrainingLessonId { get; set; }

        public int TrainingLessonCategoryId { get; set; }
        public virtual TrainingLessonCategory TrainingLessonCategory { get; set; }

        [LocalizedDisplayName("Lesson identifier")]
        public string Identifier { get; set; }

        [LocalizedDisplayName("Lesson description")]
        public string Description { get; set; }

        [LocalizedDisplayName("Requires flight")]
        public bool RequiresFlight { get; set; } = true;

        [LocalizedDisplayName("Requires flight instructor approval")]
        public bool RequiresFlightInstructorApproval { get; set; } = true;

        [LocalizedDisplayName("Enabled")]
        public bool Enabled { get; set; } = true;

        [LocalizedDisplayName("Sort order")]
        public int SortOrder { get; set; }
    }
}