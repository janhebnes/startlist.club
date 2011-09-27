using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    public class PilotStatusType
    {
        [Key]
        public int PilotStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public int? ClubId { get; set; }
        public virtual Club Club { get; set; }
    }
}
