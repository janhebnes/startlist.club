using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    using System.Xml.Serialization;

    public class Note
    {
        public int NoteId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Description { get; set; }

        // Note: The two lines below are what happens per default if it is a simple relation
        //public Guid Flight_FlightId { get; set; }
        //[ForeignKey("Flight_FlightId")]
        [XmlIgnore]
        public virtual Flight Flight { get; set; }
    }
}