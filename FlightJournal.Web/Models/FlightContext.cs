using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace FlightJournal.Web.Models
{
    public class FlightContext : DbContext
    {
        public FlightContext() : base("FlightJournal")
        {
            //Database.SetInitializer<FlightContext>(new CreateDatabaseIfNotExists<FlightContext>());
            //Database.SetInitializer<FlightContext>(new FlightContextInitializer());
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightVersionHistory> FlightVersions { get; set; }
        public DbSet<Plane> Planes { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<PilotStatusType> PilotStatusTypes { get; set; }
        public DbSet<StartType> StartTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<PilotLogEntry> PilotLogEntries { get; set; }

        /// <summary>
        /// Throw Validation Errors from the Entity as actual Exceptions
        /// </summary>
        public void ThrowValidationErrors()
        {
            string errors = string.Empty;
            this.GetValidationErrors().ToList().ForEach(b => b.ValidationErrors.ToList().ForEach(c => errors = c.PropertyName + ": " + c.ErrorMessage));
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

            this.ChangeTracker.Entries<Flight>().Where(f => (f.State == EntityState.Added) || (f.State == EntityState.Deleted) || (f.State == EntityState.Modified))
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
        /// HACK: Enabled only when working on database structure on the solution in development otherwise it might remove the database data 
        ////public class FlightContextInitializer : DropCreateDatabaseAlways<FlightContext>
        ////{
        ////    protected override void Seed(FlightContext context)
        ////    {
        ////        //// --------- Basic user information 
        ////        ////var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        ////        ////var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        ////        ////string name = "Admin";
        ////        ////string password = "123456";
        ////        ////string test = "test";

        ////        //////Create Role Test and User Test
        ////        ////RoleManager.Create(new IdentityRole(test));
        ////        ////UserManager.Create(new ApplicationUser() { UserName = test });

        ////        //////Create Role Admin if it does not exist
        ////        ////if (!RoleManager.RoleExists(name))
        ////        ////{
        ////        ////    var roleresult = RoleManager.Create(new IdentityRole(name));
        ////        ////}

        ////        //////Create User=Admin with password=123456
        ////        ////var user = new ApplicationUser();
        ////        ////user.UserName = name;
        ////        ////var adminresult = UserManager.Create(user, password);

        ////        //////Add User Admin to Role Admin
        ////        ////if (adminresult.Succeeded)
        ////        ////{
        ////        ////    var result = UserManager.AddToRole(user.Id, name);
        ////        ////}

        ////        //// --------- Basic Flight information 

        ////        // Locations
        ////        var xloc = new Location { Name = "Kongsted" };
        ////        context.Locations.Add(xloc);
        ////        context.SaveChanges();

        ////        // Clubs
        ////        var xclub = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", Location = xloc };
        ////        context.Clubs.Add(xclub);
        ////        context.SaveChanges();

        ////        // StartType
        ////        var xstart = new StartType() { Name = "Spilstart", ShortName = "S" };
        ////        context.StartTypes.Add(xstart);
        ////        base.Seed(context);
                
        ////        // Locations
        ////        var loc = new Location { Name = "Kongsted" };
        ////        context.Locations.Add(loc);
        ////        context.Locations.Add(new Location() { Name = "Arnborg" });
        ////        context.SaveChanges();

        ////        // Clubs
        ////        var club = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", Location = loc };
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
        ////                        Departure = DateTime.Now.AddHours(-3),
        ////                        Landing = DateTime.Now.AddHours(-3).AddMinutes(15),
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
        ////                        Departure = DateTime.Now.AddHours(-2),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString(),
        ////                        Description = "Some more flight description that is very long and complicated and must be handled by the display lists..."
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = DateTime.Now.AddHours(-1),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString()
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = DateTime.Now.AddHours(-4),
        ////                        Plane = pl2,
        ////                        StartedFrom = loc,
        ////                        Pilot = pilot,
        ////                        StartType = start,
        ////                        LastUpdatedBy = pilot.ToString(),
        ////                        Description = "Some flight description..."
        ////                    },
        ////                new Flight
        ////                    {
        ////                        Departure = DateTime.Now.AddHours(-3).AddMinutes(10),
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
