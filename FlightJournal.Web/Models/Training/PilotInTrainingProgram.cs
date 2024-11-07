using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    public class PilotInTrainingProgram
    {
        [Key]
        public int PilotInTrainingProgramId { get; set; }

        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }
        public int Training2ProgramId { get; set; }
        public virtual Catalogue.Training2Program Program { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}