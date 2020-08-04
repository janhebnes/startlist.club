using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using FlightJournal.Web.Migrations.FlightContext;
using FlightJournal.Web.Models.Training;

namespace FlightJournal.Web.Models
{
    public class FlightContext : DbContext
    {
        public FlightContext() : base("FlightJournal")
        {
            Database.SetInitializer<FlightContext>(new MigrateDatabaseToLatestVersion<FlightContext, Migrations.FlightContext.Configuration>());
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightVersionHistory> FlightVersions { get; set; }
        public DbSet<Plane> Planes { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<PilotStatusType> PilotStatusTypes { get; set; }
        public DbSet<StartType> StartTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<PilotLogEntry> PilotLogEntries { get; set; }
        //public DbSet<PilotLogEntryVersionHistory> PilotLogEntryVersions { get; set; }

        public DbSet<Training.Training2Program> TrainingPrograms { get; set; }
        public DbSet<Training.AppliedExercise> AppliedExercises { get; set; }
        public DbSet<Training.TrainingFlightAnnotation> TrainingFlightAnnotations { get; set; }

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

            modelBuilder.Entity<PilotLogEntry>()
                        .HasRequired(i => i.Pilot)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<PilotLogEntry>()
                        .HasOptional(i => i.Flight)
                        .WithMany()
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<PilotLogEntry>()
                        .HasOptional(i => i.TrainingLessonApprovedByFlightInstructor)
                        .WithMany()
                        .WillCascadeOnDelete(false);
        }
    }
}
