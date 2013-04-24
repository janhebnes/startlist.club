using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    using System.Xml.Serialization;

    using FlightLog.Controllers;

    public class Club
    {
        [Key]
        public int ClubId { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }

        public int DefaultStartLocationId { get; set; }
        public virtual Location DefaultStartLocation { get; set; }

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