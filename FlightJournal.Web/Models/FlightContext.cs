﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using CsvHelper.Configuration;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models
{
    public class FlightContext : DbContext
    {
        public FlightContext() : base("FlightJournal")
        {
            Database.SetInitializer<FlightContext>(new MigrateDatabaseToLatestVersion<FlightContext, Migrations.FlightContext.Configuration>());
            //Database.SetInitializer<FlightContext>(new DropCreateDatabaseAlways<FlightContext>()); // Note that after a Drop the MigrateDatabaseToLatestVersion needs to run for seed data to be populated
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

        public DbSet<Training2Program> TrainingPrograms { get; set; }
        public DbSet<Training2Lesson> TrainingLessons { get; set; }
        public DbSet<Training2Exercise> TrainingExercises{ get; set; }
        public DbSet<AppliedExercise> AppliedExercises { get; set; }
        public DbSet<TrainingFlightAnnotation> TrainingFlightAnnotations { get; set; }

        public DbSet<Manouvre> Manouvres { get; set; }
        //public DbSet<ManouvreIcon> ManouvreIcons { get; set; }
        public DbSet<WindDirection> WindDirections { get; set; }
        public DbSet<WindSpeed> WindSpeeds { get; set; }
        public DbSet<Commentary> Commentaries { get; set; }
        public DbSet<CommentaryType> CommentaryTypes { get; set; }
        public DbSet<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; set; }
        public DbSet<Grading> Gradings { get; set; }
        public DbSet<ListenerArea> ListenerAreas { get; set; }

        public DbSet<PilotInTrainingProgram> PilotsInTrainingPrograms { get; set; }

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

 /*           modelBuilder.Entity<Manouvre>()
                        .HasOptional(i => i.ManouvreIcon)
                        .WithMany()
                        .WillCascadeOnDelete(false);*/

            modelBuilder.Entity<Commentary>()
                        .HasMany<CommentaryType>(s => s.CommentaryTypes)
                        .WithMany(c => c.Commentaries)
                        .Map(cs =>
                        {
                            cs.MapLeftKey("CommentaryRefId");
                            cs.MapRightKey("CommentaryTypeRefId");
                            cs.ToTable("CommentaryCommentaryTypes");
                        });

            modelBuilder.Entity<TrainingFlightAnnotation>()
                .HasMany<Manouvre>(s => s.Manouvres)
                .WithMany(c => c.TrainingFlightAnnotations)
                .Map(cs =>
                {
                    cs.MapLeftKey("TrainingFlightAnnotationRefId");
                    cs.MapRightKey("ManouvreRefId");
                    cs.ToTable("TrainingFlightAnnotationsManouvres");
                });

            modelBuilder.Entity<TrainingFlightAnnotationCommentCommentType>()
                .HasKey(k => new { k.TrainingFlightAnnotationId, k.CommentaryId, k.CommentaryTypeId });

            modelBuilder.Entity<TrainingFlightAnnotationCommentCommentType>()
                .HasRequired(i => i.TrainingFlightAnnotation)
                .WithMany(i => i.TrainingFlightAnnotationCommentCommentTypes)
                .HasForeignKey(i => i.TrainingFlightAnnotationId);

            modelBuilder.Entity<TrainingFlightAnnotationCommentCommentType>()
                .HasRequired(i => i.Commentary)
                .WithMany(i => i.TrainingFlightAnnotationCommentCommentTypes)
                .HasForeignKey(i => i.CommentaryId);

            modelBuilder.Entity<TrainingFlightAnnotationCommentCommentType>()
                .HasRequired(i => i.CommentaryType)
                .WithMany(i => i.TrainingFlightAnnotationCommentCommentTypes)
                .HasForeignKey(i => i.CommentaryTypeId);

            //modelBuilder.Entity<AppliedExercise>()
            //    .HasIndex(ae=>new {ae.FlightId})
            //    ;
            //modelBuilder.Entity<Flight>()
            //    .HasIndex(f=>new {f.PilotId})
            //    ;
        }
    }
}
