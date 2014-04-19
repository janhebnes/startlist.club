using System.Collections.Generic;
using FlightLog.Models;

namespace FlightLog.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FlightLog.Models.FlightContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(FlightLog.Models.FlightContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

#if DEBUG
            // Locations
            var xloc = new Location { Name = "Kongsted" };
            context.Locations.AddOrUpdate(xloc);
            context.SaveChanges();

            // Clubs
            var xclub = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", DefaultStartLocation = xloc };
            context.Clubs.AddOrUpdate(xclub);
            context.SaveChanges();

            // StartType
            var xstart = new StartType() { Name = "Spilstart", ShortName = "S" };
            context.StartTypes.AddOrUpdate(xstart);
            base.Seed(context);
            
            // Locations
            var loc = new Location { Name = "Kongsted" };
            context.Locations.AddOrUpdate(loc);
            context.Locations.AddOrUpdate(new Location() { Name = "Arnborg" });
            context.SaveChanges();

            // Clubs
            var club = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", DefaultStartLocation = loc };
            context.Clubs.AddOrUpdate(club);
            context.SaveChanges();

            // StartType
            var start = new StartType() { Name = "Spilstart", ShortName = "S" };
            context.StartTypes.AddOrUpdate(start);
            context.StartTypes.AddOrUpdate(new StartType() { Name = "Flyslæb", ShortName = "F" });
            context.StartTypes.AddOrUpdate(new StartType() { Name = "Selvstart", ShortName = "M" });
            context.SaveChanges();

            context.StartTypes.AddOrUpdate(new StartType() { Name = "Passagerstart", ShortName = "Pass.", Club = club });
            context.StartTypes.AddOrUpdate(new StartType() { Name = "Gratisstart", ShortName = "Gratis", Club = club });
            context.SaveChanges();

            // Pilot status
            var y = new List<string>() { "Ingen", "Prøvemedlem", "Elev", "S-Certifikat", "Instruktør" };
            y.ForEach(b => context.PilotStatusTypes.AddOrUpdate(new PilotStatusType() { Name = b }));
            context.SaveChanges();

            // Planes
            var pl2 = new Plane { CompetitionId = "R2", Registration = "OY-XMO", Seats = 2, DefaultStartType = start, Engines = 0, EntryDate = DateTime.Now };
            context.Planes.AddOrUpdate(pl2);
            var pla = new Plane { CompetitionId = "RR", Registration = "OY-RRX", Seats = 2, DefaultStartType = start, Engines = 1, EntryDate = DateTime.Now };
            context.Planes.AddOrUpdate(pla);
            var pl1 = new Plane { CompetitionId = "PU", Registration = "OY-XPU", Seats = 1, DefaultStartType = start, Engines = 0, EntryDate = DateTime.Now };
            context.Planes.AddOrUpdate(pl1);
            context.SaveChanges();

            // Pilots
            var pilot = new Pilot { Name = "Jan Hebnes", MemberId = "1241", Club = club };
            context.Pilots.AddOrUpdate(pilot);

            context.SaveChanges();
            var s = new List<Flight>
                        {
                            new Flight
                                {
                                    Departure = new DateTime(2011, 5, 1, 23, 10, 0),
                                    Landing = new DateTime(2011, 5, 1, 23, 15, 0),
                                    Plane = pl1,
                                    StartedFrom = loc,
                                    LandedOn = loc, 
                                    Pilot = pilot,
                                    StartType = start,
                                    LastUpdatedBy = pilot.ToString()
                                },
                            new Flight
                                {
                                    Plane = pl2,
                                    StartedFrom = loc,
                                    Pilot = pilot,
                                    StartType = start,
                                    LastUpdatedBy = pilot.ToString()
                                },
                            new Flight
                                {
                                    Departure = new DateTime(2011, 5, 1, 23, 25, 0),
                                    Plane = pl2,
                                    StartedFrom = loc,
                                    Pilot = pilot,
                                    StartType = start,
                                    LastUpdatedBy = pilot.ToString(),
                                    Description = "Some more flight description that is very long and complicated and must be handled by the display lists..."
                                },
                            new Flight
                                {
                                    Departure = new DateTime(2011, 4, 15, 23, 25, 0),
                                    Plane = pl2,
                                    StartedFrom = loc,
                                    Pilot = pilot,
                                    StartType = start,
                                    LastUpdatedBy = pilot.ToString()
                                },
                            new Flight
                                {
                                    Departure = new DateTime(2011, 4, 15, 23, 25, 0),
                                    Plane = pl2,
                                    StartedFrom = loc,
                                    Pilot = pilot,
                                    StartType = start,
                                    LastUpdatedBy = pilot.ToString(),
                                    Description = "Some flight description..."
                                },
                            new Flight
                                {
                                    Departure = new DateTime(2011, 4, 5, 23, 25, 0),
                                    Plane = pl2,
                                    StartedFrom = loc,
                                    Pilot = pilot,
                                    StartType = start,
                                    LastUpdatedBy = pilot.ToString()
                                }
                        };
            s.ForEach(b => context.Flights.AddOrUpdate(b));
            context.SaveChanges();
            //base.Seed(context);

#endif

        }
    }
}
