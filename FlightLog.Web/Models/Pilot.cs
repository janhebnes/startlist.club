using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    public class Pilot 
    {
        public int PilotId { get; set; }
        [Required]
        public string Name { get; set; }
        public string UnionId { get; set; }
        public string MemberId { get; set; }
        
        public int ClubId { get; set; }
        public virtual Club Club { get; set; }

        public PilotStatusType PilotStatus { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.Name, this.MemberId);
        }
        
        public virtual ICollection<Flight> Flights { get; set; }
        public virtual ICollection<Flight> Flights_Backseat { get; set; }
        public virtual ICollection<Flight> Flights_Betaler { get; set; }
        public virtual ICollection<PilotLog> PilotLogs { get; set; }

        public virtual ICollection<FlightVersionHistory> FlightHistory_Pilots { get; set; }
        public virtual ICollection<FlightVersionHistory> FlightHistory_PilotBackseats { get; set; }
        public virtual ICollection<FlightVersionHistory> FlightHistory_Betalers { get; set; }
    }
}
