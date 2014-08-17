using System.Collections.Generic;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Migrations.FlightContext
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FlightJournal.Web.Models.FlightContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false; 
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"Migrations\FlightContext";
        }

        protected override void Seed(FlightJournal.Web.Models.FlightContext context)
        {
            //  This method will be called after migrating to the latest version.
            
            //  Only seed if the database is empty
            if (!context.StartTypes.Any() 
                && (!context.Clubs.Any())
                && (!context.Pilots.Any())
                && (!context.Planes.Any()))
            {
                InitializeDemoFlights(context);
            }

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
        }

        internal static void InitializeDemoFlights(Models.FlightContext context)
        {
            // StartType
            var start = new StartType() { Name = "Spilstart", ShortName = "S" };
            context.StartTypes.Add(start);
            context.StartTypes.Add(new StartType() { Name = "Flyslæb", ShortName = "F" });
            context.StartTypes.Add(new StartType() { Name = "Selvstart", ShortName = "M" });
            context.SaveChanges();

            // Locations
            var location = new Location { Name = "Kongsted" };
            context.Locations.Add(location);
            var location2 = new Location { Name = "True" };
            context.Locations.Add(location2);
            context.Locations.Add(new Location() { Name = "Arnborg" });
            context.SaveChanges();

            // Clubs
            var club = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", Location = location };
            context.Clubs.Add(club);
            var club2 = new Club() { ClubId = 99, ShortName = "AASVK", Name = "Århus Svæveflyveklub", Location = location2 };
            context.Clubs.Add(club2);
            context.SaveChanges();

            // Planes
            var pl2 = new Plane
            {
                CompetitionId = "R2",
                Registration = "OY-XMO",
                Type = "ASK21",
                Seats = 2,
                DefaultStartType = start,
                Engines = 0
            };
            context.Planes.Add(pl2);
            var pla = new Plane
            {
                CompetitionId = "RR",
                Registration = "OY-RRX",
                Class = "Open",
                Type = "Duo Discus",
                Seats = 2,
                DefaultStartType = start,
                Engines = 1
            };
            context.Planes.Add(pla);
            var pl1 = new Plane
            {
                CompetitionId = "PU",
                Registration = "OY-XPU",
                Class = "15-Meter",
                Type = "LS6",
                Model = "LS6a",
                Seats = 1,
                DefaultStartType = start,
                Engines = 0
            };
            context.Planes.Add(pl1);
            context.SaveChanges();

            // Pilots
            var pilot = new Pilot { Name = "Jan Hebnes", MemberId = "1241", Club = club, Email = "jan.hebnes@gmail.com", MobilNumber = "+4524250682" };
            context.Pilots.Add(pilot);
            context.Pilots.Add(new Pilot { Name = "Søren Sarup", MemberId = "1125", Club = club });

            context.SaveChanges();
            var s = new List<Flight>
                {
                    new Flight
                    {
                        Departure = DateTime.Now.AddHours(-3),
                        Landing = DateTime.Now.AddHours(-3).AddMinutes(15),
                        Plane = pl1,
                        StartedFrom = location,
                        LandedOn = location,
                        Pilot = pilot,
                        StartType = start,
                        Description = "Demo flight",
                        LastUpdatedBy = pilot.ToString()
                    },
                    new Flight
                    {
                        Plane = pl2,
                        StartedFrom = location,
                        Pilot = pilot,
                        StartType = start,
                        Description = "Demo flight (admin@admin.com/Admin@123456)",
                        LastUpdatedBy = pilot.ToString()
                    },
                    new Flight
                    {
                        Departure = DateTime.Now.AddHours(-2),
                        Plane = pl2,
                        StartedFrom = location,
                        Pilot = pilot,
                        StartType = start,
                        LastUpdatedBy = pilot.ToString(),
                        Description = "Demo flight"
                    },
                    new Flight
                    {
                        Departure = DateTime.Now.AddHours(-1),
                        Plane = pl2,
                        StartedFrom = location,
                        Pilot = pilot,
                        StartType = start,
                        Description = "Demo flight",
                        LastUpdatedBy = pilot.ToString()
                    },
                    new Flight
                    {
                        Departure = DateTime.Now.AddHours(-4),
                        Plane = pl2,
                        StartedFrom = location,
                        Pilot = pilot,
                        StartType = start,
                        LastUpdatedBy = pilot.ToString(),
                        Description = "Demo flight"
                    },
                    new Flight
                    {
                        Departure = DateTime.Now.AddHours(-3).AddMinutes(10),
                        Plane = pl2,
                        StartedFrom = location,
                        Pilot = pilot,
                        StartType = start,
                        Description = "Demo flight",
                        LastUpdatedBy = pilot.ToString()
                    }
                };
            s.ForEach(b => context.Flights.Add(b));
            context.SaveChanges();
        }
    }
}
