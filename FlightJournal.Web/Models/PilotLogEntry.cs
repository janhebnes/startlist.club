using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models
{
    public class PilotLogEntry
    {
        [Key]
        public Guid PilotLogid { get; set; }
        public string Lesson { get; set; }
        public string Description { get; set; }

        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }

        public Guid FlightId { get; set; }
        public virtual Flight Flight { get; set; }
    }
}
