using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace FlightJournal.Web.Models
{
    public class Plane
    {
        public int PlaneId { get; set; }
        [Required]
        public string Registration { get; set; }
        [Required]
        public string CompetitionId { get; set; }

        /// <summary>
        /// Overrides registration and competition (Must be unique)
        /// </summary>
        public string ShortName { get; set; }

        //public string Class { get; set; }
        //public string Type { get; set; }
        //public string Model { get; set; }

        [Required, Range(1,2)]
        public int Seats { get; set; }
        [Required, Range(0, 1)]
        public int Engines { get; set; }

        public DateTime? ExitDate { get; set; }

        public int? StartTypeId { get; set; }

        [XmlIgnore]
        public string RenderName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.ShortName))
                {
                    return this.ShortName;
                }
                return string.Format("{0} ({1})", this.CompetitionId, this.Registration);
            }
        }

        [XmlIgnore]
        public virtual StartType DefaultStartType { get; set; }

        [XmlIgnore]
        public virtual ICollection<Flight> Flights { get; set; }

        public override string ToString()
        {
            return this.RenderName;
        }
    }
}
