using System;
using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class Grading
    {
        [Key]
        public int GradingId { get; set; }
        public Guid GradingIdForExport { get; set; }

        [LocalizedDisplayName("Comment/evaluation")]
        public string Name { get; set; }
        [LocalizedDisplayName("Comment/Numerical grade (higher is better)")]
        public int Value { get; set; }

        [LocalizedDisplayName("Is this the 'OK' or 'Completed' grading?")]
        public bool IsOk { get; set; }

        [LocalizedDisplayName("Does this apply to 'Briefing only' partial exercises?")]
        public bool AppliesToBriefingOnlyPartialExercises { get; set; }
        
        [LocalizedDisplayName("Does this apply to practical partial exercises?")]
        public bool AppliesToPracticalPartialExercises { get; set; }

        public int DisplayOrder { get; set; }

    }
}