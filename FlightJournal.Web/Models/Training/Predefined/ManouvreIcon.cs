using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class ManouvreIcon
    {
        [Key]
        public int IconId { get; set; }
        public string Icon { get; set; }
        //public virtual IEnumerable<Manouvre> Manouvres { get; set; } 
    }
}