using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string Name {get; set; }

        //public virtual ICollection<Flight> Flights { get; set; }
        public override string ToString()
        {
            return this.Name;
        }
    }
}
