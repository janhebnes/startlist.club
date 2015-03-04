using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace FlightJournal.Web.Models
{
    public class Pilot 
    {
        public int PilotId { get; set; }
        [Required]
        public string Name { get; set; }
        [XmlIgnore]
        public string RenderName 
        {
            get
            {
                return string.Format("{0} ({1})", this.Name, this.MemberId);
            }
        }
        public string UnionId { get; set; }
        public string MemberId { get; set; }
        public string MobilNumber { get; set; }
        public string Email { get; set; }
        
        public int ClubId { get; set; }
        public virtual Club Club { get; set; }

        public string Note { get; set; }
        public DateTime? ExitDate { get; set; }

        public PilotStatusType PilotStatus { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.Name, this.MemberId);
        }

        [XmlIgnore]
        public virtual ICollection<Flight> Flights { get; set; }
        [XmlIgnore]
        public virtual ICollection<Flight> Flights_Backseat { get; set; }
        [XmlIgnore]
        public virtual ICollection<Flight> Flights_Betaler { get; set; }
        //[XmlIgnore]
        //public virtual ICollection<PilotLogEntry> PilotLogEntries { get; set; }

        [XmlIgnore]
        public virtual ICollection<FlightVersionHistory> FlightHistory_Pilots { get; set; }
        [XmlIgnore]
        public virtual ICollection<FlightVersionHistory> FlightHistory_PilotBackseats { get; set; }
        [XmlIgnore]
        public virtual ICollection<FlightVersionHistory> FlightHistory_Betalers { get; set; }

    }
}
