using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

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

        public string Lesson { get; set; }
        public string Description { get; set; }
    }
}
