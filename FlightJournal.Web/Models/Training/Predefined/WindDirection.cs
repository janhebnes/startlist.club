using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class WindDirection
    {
        [Key]
        public int WindDirectionId { get; set; }
        public int WindDirectionItem { get; set; }
    }
}