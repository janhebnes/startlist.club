using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class Manouvre
    {
        [Key]
        public int ManouvreId { get; set; }
        public string ManouvreItem { get; set; }
        public int Icon { get; set; }
        public virtual ManouvreIcon ManouvreIcon {get; set;}
    }
}