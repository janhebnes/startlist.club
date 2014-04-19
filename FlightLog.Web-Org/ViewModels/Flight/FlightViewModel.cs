using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlightLog.Models;

namespace FlightLog.ViewModels.Flight
{
    public class FlightViewModel
    {
        public string Title { get; set; }
        public double Price { get; set; }
        //public FlightViewModel(FlightLog.Models.Flight flight)
        //{
        //    FlightId = flight.FlightId;
        //    PlaneId = flight.PlaneId;
        //    Description = flight.Description;
        //}

        //public Guid FlightId { get; set; }
        //public DateTime? Departure{ get; set; }
        //public DateTime? Landing { get; set; }
        //public int PlaneId { get; set; }
        //public int PilotId { get; set; }
        //public int? PilotBackseatId { get; set; }
        //public int StartTypeId { get; set; }
        //public int StartedFromId { get; set; }
        //public int? LandedOnId { get; set; }
        //public double? TachoDeparture { get; set; }
        //public double? TachoLanding { get; set; }
        //public double? TaskDistance { get; set; }
        //public string Description { get; set; }
        //public int BetalerId { get; set; }
    }
}