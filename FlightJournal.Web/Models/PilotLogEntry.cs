using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class PilotLogEntry
    {
        [Key]
        public Guid PilotLogid { get; set; }

        [Required]
        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }

        public int FlightId { get; set; }
        public virtual Flight Flight { get; set; }

        // Flight classification/type  FlightSchool (Elev/skoling) / ProficiencyCheck (Færdighedstræning) / Familiarisation training (Omskoling) / Differences training (Omskoling m/Instruktør)

        public PilotPosition Position { get; set; }

        [LocalizedDisplayName("Description")]
        public string Description { get; set; }

        // FlightTrainingLessonId (TMG ? S ? A normer B normer ? Omskolinger til startmetoder ? Type omskolinger ? )
        // FlightTrainingLessonApprovedByPilotId (int Pilot approving the lesson (on solo flights this might not be an FI onboard))
        // FlightTrainingLesson also has aspects not controlled by a flight 

        [LocalizedDisplayName("Lesson")]
        public int TrainingLessonId { get; set; }
        public virtual TrainingLesson TrainingLesson { get; set; }

        [LocalizedDisplayName("Lesson approved")]
        public DateTime? TrainingLessonApproved { get; set; }

        [LocalizedDisplayName("Lesson approved by")]
        public int? TrainingLessonApprovedByFlightInstructorId { get; set; }
        public virtual Instructor TrainingLessonApprovedByFlightInstructor { get; set; }
        
        [LocalizedDisplayName("Deleted")]
        public DateTime? Deleted { get; set; }

        [Required]
        [LocalizedDisplayName("Last updated")]
        public DateTime LastUpdated { get; set; }
        [LocalizedDisplayName("Last updated by")]
        public string LastUpdatedBy { get; set; }

    }
}
