using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Models.Training.Flight
{
    public class Training
    {
        [Key]
        [Column(Order=1)]
        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }

        [Key]
        [Column(Order=2)]
        public int Training2ProgramId { get; set; }
        public virtual Training2Program TrainingProgram { get; set; }

        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public bool DidComplete { get; set; }
    }
}