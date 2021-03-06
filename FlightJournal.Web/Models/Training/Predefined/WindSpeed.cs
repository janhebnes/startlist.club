using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class WindSpeed
    {
        [Key]
        public int WindSpeedId { get; set; }
        public int WindSpeedItem { get; set; }
    }
}