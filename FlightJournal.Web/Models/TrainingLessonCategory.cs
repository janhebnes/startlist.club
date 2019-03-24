using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class TrainingLessonCategory
    {
        [Key]
        public int TrainingLessonCategoryId { get; set; }

        [LocalizedDisplayName("Lesson category")]
        public string TrainingLessonCategoryName { get; set; }

        [LocalizedDisplayName("Enabled")]
        public bool Enabled { get; set; } = true;

        [LocalizedDisplayName("Sort order")]
        public int SortOrder { get; set; }
    }
}