using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class Pilot 
    {
        public int PilotId { get; set; }
        [Required]
        [LocalizedDisplayName("Pilot name")]
        public string Name { get; set; }
        [LocalizedDisplayName("Union id")]
        public string UnionId { get; set; }
        [LocalizedDisplayName("Club id")]
        public string MemberId { get; set; }
        [LocalizedDisplayName("Mobil number")]
        public string MobilNumber { get; set; }
        [LocalizedDisplayName("Email")]
        public string Email { get; set; }
        
        public int ClubId { get; set; }
        [LocalizedDisplayName("Club")]
        public virtual Club Club { get; set; }
        
        [LocalizedDisplayName("Note")]
        public string Note { get; set; }
        [LocalizedDisplayName("Exit date")]
        public DateTime? ExitDate { get; set; }

        [LocalizedDisplayName("Pilot status")]
        public PilotStatusType PilotStatus { get; set; }

        /// <summary>
        /// Required by SelectList logic in FlightController
        /// </summary>
        [XmlIgnore]
        public string RenderName 
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            return ToString(true);
        }
        
        public string ToString(bool includeClub)
        {
            var clubName = string.Empty;
            if (includeClub && this.Club != null)
            {
                clubName = " " + this.Club.ShortName;
            }

            if (!string.IsNullOrWhiteSpace(this.MemberId))
            {
                return string.Format("{0} ({1}){2}", this.Name, this.MemberId, clubName);
            }
            else if (!string.IsNullOrWhiteSpace(this.UnionId))
            {
                return string.Format("{0} ({1}){2}", this.Name, this.UnionId, clubName);
            }
            else
            {
                return this.Name + clubName;
            }
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
