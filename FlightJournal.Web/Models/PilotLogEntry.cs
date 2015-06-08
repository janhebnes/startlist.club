using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class PilotLogEntry
    {
        [Key]
        public Guid PilotLogid { get; set; }
        
        [Required]
        [XmlIgnore]
        public virtual Pilot Pilot { get; set; }

        [Required]
        [XmlIgnore]
        public virtual Flight Flight { get; set; }

        [LocalizedDisplayName("Lesson")]
        public string Lesson { get; set; }
        [LocalizedDisplayName("Description")]
        public string Description { get; set; }
    }
}
