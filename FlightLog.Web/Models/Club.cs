using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    public class Club
    {
        [Key]
        public int ClubId { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }

        public int DefaultStartLocationId { get; set; }
        public virtual Location DefaultStartLocation { get; set; }

        public virtual ICollection<StartType> StartTypes { get; set; }
        public virtual ICollection<PilotStatusType> PilotStatusTypes { get; set; }
        public virtual ICollection<Pilot> Pilots { get; set; }
    }
}