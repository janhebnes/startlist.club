using FlightJournal.Web.Models.Training.Predefined;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training.Flight
{
    public class Weather
    {
        [Key]
        public int WeatherId { get; set; }
        public virtual WindDirection WindDirection { get; set; }
        public virtual WindSpeed WindSpeed { get; set; }
    }
}