using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    public class PilotLog
    {
        [Key]
        public Guid PilotLogid { get; set; }
        public string Lesson { get; set; }
        public string Description { get; set; }

        //public int PilotId { get; set; }
        //public virtual Pilot Pilot { get; set; }

        //public Guid FlightId { get; set; }
        //public virtual Flight Flight { get; set; }
    }
}
