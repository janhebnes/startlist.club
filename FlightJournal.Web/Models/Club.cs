using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using FlightJournal.Web.Controllers;

namespace FlightJournal.Web.Models
{
    public class Club
    {
        [Key]
        public int ClubId { get; set; }
        [DisplayName("Forkortelse")]
        public string ShortName { get; set; }
        [DisplayName("Fulde klubnavn")]
        public string Name { get; set; }

        [DisplayName("Sted")]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        [XmlIgnore]
        public virtual ICollection<StartType> StartTypes { get; set; }
        [XmlIgnore]
        public virtual ICollection<PilotStatusType> PilotStatusTypes { get; set; }
        [XmlIgnore]
        public virtual ICollection<Pilot> Pilots { get; set; }

        /// <summary>
        /// Return true if the current Club is the Currently Selected Club or if no club is selected
        /// </summary>
        /// <returns></returns>
        public bool IsCurrent()
        {
            return (ClubController.CurrentClub.ShortName == null || ClubController.CurrentClub.ShortName == this.ShortName);
        }
    }
}