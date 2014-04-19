using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace FlightLog.APIs
{
    using FlightLog.Models;

    [ServiceContract]
    public class FlightsApi
    {
        [WebGet(UriTemplate = "")]
        public IQueryable<Flight> Get()
        {
            ////var flight = new Flight() { Date = DateTime.Now, Pilot = new Pilot() { Name = "Jan"}};
            ////return (new List<Flight>() { flight }).AsQueryable();

            var db = new FlightContext();
            //////DateTime? date, int? locationid, int? skip, int? take
            ////var flights = db.Flights.Where(s => locationid.HasValue ? (s.LandedOn.LocationId == locationid.Value || s.StartedFrom.LocationId == locationid.Value) : true).OrderByDescending(s => s.Date).ThenByDescending(s => s.Departure ?? DateTime.Now).Skip((skip.HasValue ? skip.Value : 0)).Take((take.HasValue ? take.Value : 100));
            var flights = db.Flights.Include("Plane").Include("Pilot").Include("StartedFrom").Include("LandedOn").OrderByDescending(s => s.Date).ThenByDescending(s => s.Departure ?? DateTime.Now).Take(100);

            return flights.ToList().AsQueryable();

        ////    var flight = new Flight() { Date = DateTime.Now, Pilot = new Pilot() { Name = "Jan"}};
        ////    var contacts = new List<Contact>()
        ////{
        ////    new Contact {ContactId = 1, Name = "Phil Haack", FlightTest = flight},
        ////    new Contact {ContactId = 2, Name = "HongMei Ge"},
        ////    new Contact {ContactId = 3, Name = "Glenn Block", FlightTest = flight},
        ////    new Contact {ContactId = 4, Name = "Howard Dierking"},
        ////    new Contact {ContactId = 5, Name = "Jeff Handley"},
        ////    new Contact {ContactId = 6, Name = "Yavor Georgiev"}
        ////};
        ////    return contacts.AsQueryable();
        }
    }

    //public class Contact
    //{
    //    public int ContactId { get; set; }
    //    public string Name { get; set; }
    //    public Flight FlightTest { get; set; }
    //}
}