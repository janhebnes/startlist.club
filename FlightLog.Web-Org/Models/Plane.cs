using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    using System.Xml.Serialization;

    public class Plane
    {
        public int PlaneId { get; set; }
        [Required]
        public string Registration { get; set; }
        [Required]
        public string CompetitionId { get; set; }
        
        [Required, Range(1,2)]
        public double Seats { get; set; }
        [Required, Range(0, 1)]
        public double Engines { get; set; }

        public DateTime EntryDate { get; set; }
        public DateTime? ExitDate { get; set; }

        public int? StartTypeId { get; set; }

        [XmlIgnore]
        public string RenderName
        {
            get
            {
                return string.Format("{0} ({1})", this.CompetitionId, this.Registration);
            }
        }

        [XmlIgnore]
        public virtual StartType DefaultStartType { get; set; }

        [XmlIgnore]
        public virtual ICollection<Flight> Flights { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.CompetitionId, this.Registration);
        }
    }
}
