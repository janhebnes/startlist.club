using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class Club
    {
        [Key]
        public int ClubId { get; set; }
        [LocalizedDisplayName("Shortname")]
        public string ShortName { get; set; }
        [LocalizedDisplayName("Fullname")]
        public string Name { get; set; }

        [LocalizedDisplayName("Location")]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        [LocalizedDisplayName("Website")]
        public string Website { get; set; }

        [LocalizedDisplayName("Visible in Club filter")]
        public bool Visible { get; set; }

        [LocalizedDisplayName("Contact information")]
        public string ContactInformation { get; set; }

        [LocalizedDisplayName("Use APRS-based registration of takeoff and landing")]
        public bool UseAPRSTakeoffAndLanding { get; set; }

        public int? ExportRecipientId { get; set; }
        public virtual ExportRecipient ExportRecipient { get; set; }

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

        /// <summary>
        /// Returns the country based on the associated location
        /// </summary>
        [LocalizedDisplayName("Country")]
        public string Country
        {
            get
            {
                return Location != null ? Location.Country : null;
            }
        }
    }
}