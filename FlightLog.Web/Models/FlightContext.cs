using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace FlightLog.Models
{
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.EntityClient;
    using System.Data.Objects;
    using System.Data.SqlClient;
    using System.Web.Configuration;

    public class FlightContext : DbContext
    {
        public FlightContext()
        {   
            Database.CompatibleWithModel(true);
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightVersionHistory> FlightVersions { get; set; }
        public DbSet<Plane> Planes { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<PilotStatusType> PilotStatusTypes { get; set; }
        public DbSet<StartType> StartTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        //public DbSet<PilotLog> PilotLogs { get; set; }

        /// <summary>
        /// Throw Validation Errors from the Entity as actual Exceptions
        /// </summary>
        public void ThrowValidationErrors()
        {
            string errors = string.Empty;
            this.GetValidationErrors().ToList<System.Data.Entity.Validation.DbEntityValidationResult>().ForEach(b => b.ValidationErrors.ToList<System.Data.Entity.Validation.DbValidationError>().ForEach(c => errors = c.PropertyName + ": " + c.ErrorMessage));
            if (!string.IsNullOrEmpty(errors))
            {
                throw new Exception(errors);
            }
        }

        /// <summary>
        /// On Save we monitor if Validation errors are present and we Save flight information history automatically
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            this.ThrowValidationErrors();

            this.ChangeTracker.Entries<Flight>().Where(f => (f.State == System.Data.EntityState.Added) || (f.State == System.Data.EntityState.Deleted) || (f.State == System.Data.EntityState.Modified))
                                    .ToList<DbEntityEntry<Flight>>()
                                    .ForEach((c => this.FlightVersions.Add(new FlightVersionHistory((Flight)c.Entity, c.State))));

            return base.SaveChanges();
        }

        /// <summary>
        /// </summary>
        /// <param name="modelBuilder">
        /// The model builder.
        /// </param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flight>()
                        .HasRequired(m => m.Pilot)
                        .WithMany(t => t.Flights)
                        .HasForeignKey(m => m.PilotId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Flight>()
                        .HasOptional(m => m.PilotBackseat)
                        .WithMany(t => t.Flights_Backseat)
                        .HasForeignKey(m => m.PilotBackseatId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Flight>()
                        .HasRequired(m => m.Betaler)
                        .WithMany(t => t.Flights_Betaler)
                        .HasForeignKey(m => m.BetalerId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<FlightVersionHistory>()
                .ToTable("FlightVersionHistory");

            modelBuilder.Entity<FlightVersionHistory>()
                        .HasRequired(m => m.Pilot)
                        .WithMany(t => t.FlightHistory_Pilots)
                        .HasForeignKey(m => m.PilotId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<FlightVersionHistory>()
                        .HasOptional(m => m.PilotBackseat)
                        .WithMany(t => t.FlightHistory_PilotBackseats)
                        .HasForeignKey(m => m.PilotBackseatId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<FlightVersionHistory>()
                        .HasRequired(m => m.Betaler)
                        .WithMany(t => t.FlightHistory_Betalers)
                        .HasForeignKey(m => m.BetalerId)
                        .WillCascadeOnDelete(false);

        }

#if DEBUG
        //// /// Used for Initial tests of model prior to EF 4.3 Migration model 
        ////public class FlightContextInitializer : DropCreateDatabaseAlways<FlightContext>
        ////{
        ////    protected override void Seed(FlightContext context)
        ////    {
        ////        // Locations
        ////        var xloc = new Location { Name = "Kongsted" };
        ////        context.Locations.Add(xloc);
        ////        context.SaveChanges();

        ////        // Clubs
        ////        var xclub = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", DefaultStartLocation = xloc };
        ////        context.Clubs.Add(xclub);
        ////        context.SaveChanges();

        ////        // StartType
        ////        var xstart = new StartType() { Name = "Spilstart", ShortName = "S" };
        ////        context.StartTypes.Add(xstart);
        ////        base.Seed(context);
        ////        return;


        ////        // Locations
        ////        var loc = new Location { Name = "Kongsted" };
        ////        context.Locations.Add(loc);
        ////        context.Locations.Add(new Location() { Name = "Arnborg" });
        ////        context.SaveChanges();

        ////        // Clubs
        ////        var club = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", DefaultStartLocation = loc };
        ////        context.Clubs.Add(club);
        ////        context.SaveChanges();

        ////        // StartType
        ////        var start = new StartType() { Name = "Spilstart", ShortName = "S" };
        ////        context.StartTypes.Add(start);
        ////        context.StartTypes.Add(new StartType() { Name = "Flyslæb", ShortName = "F" });
        ////        context.StartTypes.Add(new StartType() { Name = "Selvstart", ShortName = "M" });
        ////        context.SaveChanges();

        ////        context.StartTypes.Add(new StartType() { Name = "Passagerstart", ShortName = "Pass.", Club = club });
        ////        context.StartTypes.Add(new StartType() { Name = "Gratisstart", ShortName = "Gratis", Club = club });
        ////        context.SaveChanges();

        ////        // Pilot status
        ////        var y = new List<string>() { "Ingen", "Prøvemedlem", "Elev", "S-Certifikat", "Instruktør" };
        ////        y.ForEach(b => context.PilotStatusTypes.Add(new PilotStatusType() { Name = b }));
        ////        context.SaveChanges();

        ////        // Planes
        ////        var pl2 = new Plane { CompetitionId = "R2", Registration = "OY-XMO", Seats = 2, DefaultStartType = start, Engines = 0, EntryDate = DateTime.Now };
        ////        context.Planes.Add(pl2);
        ////        var pla = new Plane { CompetitionId = "RR", Registration = "OY-RRX", Seats = 2, DefaultStartType = start, Engines = 1, EntryDate = DateTime.Now };
        ////        context.Planes.Add(pla);
        ////        var pl1 = new Plane { CompetitionId = "PU", Registration = "OY-XPU", Seats = 1, DefaultStartType = start, Engines = 0, EntryDate = DateTime.Now };
        ////        context.Planes.Add(pl1);
        ////        context.SaveChanges();

        ////        // Pilots
        ////        var pilot = new Pilot { Name = "Jan Hebnes", MemberId = "1241", Club = club };
        ////        context.Pilots.Add(pilot);

        ////        context.SaveChanges();
        ////        var s = new List<Flight>
        ////            {
        ////                new Flight
        ////                    {
        ////                        Departure = new DateTime(2011, 5, 1, 23, 10, 0),
        ////                        Landing = new DateTime(2011, 5, 1, 23, 15, 0),
        ////                        Plane = pl1,
        ////                        StartedFrom = loc,
        ////                        LandedOn = loc, 
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString()
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString()
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = new DateTime(2011, 5, 1, 23, 25, 0),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString(),
        ////                        Description = "Some more flight description that is very long and complicated and must be handled by the display lists..."
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = new DateTime(2011, 4, 15, 23, 25, 0),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString()
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = new DateTime(2011, 4, 15, 23, 25, 0),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString(),
        ////                        Description = "Some flight description..."
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = new DateTime(2011, 4, 5, 23, 25, 0),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString()
        ////                    }
        ////            };
        ////        s.ForEach(b => context.Flights.Add(b));
        ////        context.SaveChanges();
        ////        base.Seed(context);
        ////    }
        ////}
#endif
    }
}
