using System;
using System.Xml.Serialization;

namespace FlightJournal.Web.Models
{
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