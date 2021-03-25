using System.Collections.Generic;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Predefined;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using FlightJournal.Web.Models.Training.Flight;
using Newtonsoft.Json;

namespace FlightJournal.Web.Migrations.FlightContext
{
    internal sealed partial class Configuration : DbMigrationsConfiguration<FlightJournal.Web.Models.FlightContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"Migrations\FlightContext";
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
            var location = new Location { Name = "Kongsted", Country = "DK", ICAO = "EKKL" };
            context.Locations.Add(location);
            var location2 = new Location { Name = "True", Country = "DK" };
            context.Locations.Add(location2);
            var location3 = new Location { Name = "Slaglille", Country = "DK", ICAO = "EKSL" };
            context.Locations.Add(location3);
            var location4 = new Location { Name = "Tølløse" };
            context.Locations.Add(location4);
            var location5 = new Location { Name = "Martin", Country = "SK", ICAO = "LZMA" };
            context.Locations.Add(location5);

            context.Locations.Add(new Location() { Name = "Arnborg", Country = "DK", ICAO = "EK51" });
            context.SaveChanges();

            // Clubs
            var club = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", Location = location, Website = "http://flyveklubben.dk", Visible = true };
            context.Clubs.Add(club);
            var club2 = new Club() { ClubId = 99, ShortName = "AASVK", Name = "Århus Svæveflyveklub", Location = location2, Website = "http://www.aasvk.dk" };
            context.Clubs.Add(club2);
            var club3 = new Club() { ClubId = 199, ShortName = "MSF", Name = "Midtsjællands Svæveflyveklub", Location = location3, Website = "http://slaglille.dk", Visible = true};
            context.Clubs.Add(club3);
            var club4 = new Club() { ClubId = 210, ShortName = "TØL", Name = "Tølløse Flyveklub", Location = location4, Website = "http://www.cumulus.dk/" };
            context.Clubs.Add(club4);
            context.SaveChanges();

            // Planes
            var pl2 = new Plane
            {
                CompetitionId = "R2",
                Registration = "OY-XKO",
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
            var pilot = new Pilot { Name = "Jan Hebnes", MemberId = "1241", Club = club, Email = "jan.hebnes@gmail.com", MobilNumber = "+4500000000" };
            context.Pilots.Add(pilot);
            var pilot1 = new Pilot { Name = "Mr Demo Manager", MemberId = "9991", Club = club, MobilNumber = "+4500000001" };
            context.Pilots.Add(pilot1);
            var pilot2 = new Pilot { Name = "Mr Demo Editor", MemberId = "9992", Club = club, MobilNumber = "+4500000002" };
            context.Pilots.Add(pilot2);
            var pilot3 = new Pilot { Name = "Mr Demo Pilot", MemberId = "9993", Club = club, MobilNumber = "+4500000003" };
            context.Pilots.Add(pilot3);
            var pilot1B = new Pilot { Name = "Mr Demo OtherClub Manager", MemberId = "9995", Club = club3, MobilNumber = "+4500000005" };
            context.Pilots.Add(pilot1B);
            var pilot2B = new Pilot { Name = "Mr Demo OtherClub Editor", MemberId = "9996", Club = club3, MobilNumber = "+4500000006" };
            context.Pilots.Add(pilot2B);
            var pilot3B = new Pilot { Name = "Mr Demo OtherClub Pilot", MemberId = "9997", Club = club3, MobilNumber = "+4500000007" };
            context.Pilots.Add(pilot3B);

            context.SaveChanges();

            GenerateFlights(pl1, pl2, location, pilot, start)
                .ForEach(b => context.Flights.Add(b));

            GenerateFlights(pl1, pl2, location, pilot2, start)
                .ForEach(b => context.Flights.Add(b));

            GenerateFlights(pl1, pl2, location, pilot3, start)
                .ForEach(b => context.Flights.Add(b));

            GenerateFlights(pl1, pl2, location3, pilot2B, start)
                .ForEach(b => context.Flights.Add(b));

            GenerateFlights(pl1, pl2, location3, pilot3B, start)
                .ForEach(b => context.Flights.Add(b));

            GenerateFlights(pl1, pl2, location5, pilot3B, start)
                .ForEach(b => context.Flights.Add(b));

            GenerateFlights(pl1, pl2, location5, pilot3B, start)
                .ForEach(b => context.Flights.Add(b));

            context.SaveChanges();

            // Flight Training 

            var pilotStudent = new Pilot { Name = "Mr Demo Student Pilot", MemberId = "9998", Club = club, MobilNumber = "+4500000008" };
            context.Pilots.Add(pilotStudent);
            var pilotInstructor = new Pilot { Name = "Mr Demo Instructor Pilot", MemberId = "9999", Club = club, MobilNumber = "+4500000009", InstructorId = "Maverick-FI007" };
            context.Pilots.Add(pilotInstructor);

            context.SaveChanges();

            for(DateTime when = DateTime.Now.AddDays(-90); when <= DateTime.Now; when = when.AddDays(1))
               FlightTraining.GenerateTrainingFlights(when, pl2, location, pilotStudent, pilotInstructor, start, context);
        }

        protected override void Seed(FlightJournal.Web.Models.FlightContext context)
        {
            //  This method will be called after migrating to the latest version.

            var dropCreateTrainingPrograms = false;
            if (dropCreateTrainingPrograms || !context.TrainingPrograms.Any())
            {
                context.TrainingPrograms.RemoveRange(context.TrainingPrograms);
                context.TrainingLessons.RemoveRange(context.TrainingLessons);
                context.TrainingExercises.RemoveRange(context.TrainingExercises);
                context.SaveChanges();

                //FlightTraining.InitializeTrainingPrograms(context);
                FlightTraining.InitializeTrainingProgramsFromFileSystem(context, HttpContext.Current.Server.MapPath("~/DevData/SPL-certifikat, spilstart, UHB922 01012021.json"));
                FlightTraining.InitializeTrainingProgramsFromFileSystem(context, HttpContext.Current.Server.MapPath("~/DevData/SPL-certifikat, flyslæb, UHB922 01012021.json"));
                FlightTraining.InitializeTrainingProgramsFromFileSystem(context, HttpContext.Current.Server.MapPath("~/DevData/SPL-certifikat, selvstart, UHB922 01012021.json"));
                FlightTraining.InitializeTrainingProgramsFromFileSystem(context, HttpContext.Current.Server.MapPath("~/DevData/SPL-certifikat, TMG, UHB923 30012021.json"));
                FlightTraining.InitializeTrainingProgramsFromFileSystem(context, HttpContext.Current.Server.MapPath("~/DevData/TMG-rettighed.json"));
            }

            if (dropCreateTrainingPrograms || !context.Manouvres.Any())
            {
                context.Manouvres.RemoveRange(context.Manouvres);
                FlightTraining.InitializeManouvres(context);
            }

            if (dropCreateTrainingPrograms || !context.WindSpeeds.Any())
            {
                context.WindSpeeds.RemoveRange(context.WindSpeeds);
                FlightTraining.InitializeWindSpeeds(context);
            }

            if (dropCreateTrainingPrograms || !context.WindDirections.Any())
            {
                context.WindDirections.RemoveRange(context.WindDirections);
                FlightTraining.InitializeWindDirections(context);
            }

            if (dropCreateTrainingPrograms || !context.Commentaries.Any())
            {
                context.Commentaries.RemoveRange(context.Commentaries);
                context.CommentaryTypes.RemoveRange(context.CommentaryTypes);
                FlightTraining.InitializeCommentaries(context);
            }

            if (dropCreateTrainingPrograms || !context.Gradings.Any())
            {
                context.Gradings.RemoveRange(context.Gradings);
                FlightTraining.InitializeGradings(context);
            }

            //  Only seed if the database is empty
            if (!context.StartTypes.Any()
                && (!context.Clubs.Any())
                && (!context.Pilots.Any())
                && (!context.Planes.Any()))
            {
                InitializeDemoFlights(context);
            }


            /// This is a temp hack to migrate from the old brief/trained/ok to Gradings (to let testers work with existing data)
            /// Can be removed before deployment to production.

            //var notMigratedAppliedExercises = context.AppliedExercises.Where(x => x.Grading == null).ToList();
            //if (notMigratedAppliedExercises.Any())
            //{
            //    var gradings = context.Gradings.ToList();

            //    var briefedGrading = gradings.FirstOrDefault(x => x.AppliesToBriefingOnlyPartialExercises && x.IsOk);
            //    var grading1 = gradings.Where(x => x.AppliesToPracticalPartialExercises && !x.IsOk).OrderBy(x => x.Value).FirstOrDefault();
            //    var grading2 = gradings.Where(x => x.AppliesToPracticalPartialExercises && !x.IsOk).OrderBy(x => x.Value).Skip(1).FirstOrDefault();
            //    var gradingOk = gradings.Where(x => x.AppliesToPracticalPartialExercises && x.IsOk).OrderBy(x => x.Value).LastOrDefault();

            //    foreach (var ae in notMigratedAppliedExercises)
            //    {
            //        switch (ae.Action)
            //        {
            //            case ExerciseAction.None:
            //                ae.Grading = null;
            //                break;
            //            case ExerciseAction.Briefed when ae.Exercise.IsBriefingOnly:
            //                ae.Grading = briefedGrading;
            //                break;
            //            case ExerciseAction.Briefed:
            //                ae.Grading = grading1;
            //                break;
            //            case ExerciseAction.Trained:
            //                ae.Grading = grading2;
            //                break;
            //            case ExerciseAction.Completed:
            //                ae.Grading = gradingOk;
            //                break;
            //        }

            //        try
            //        {
            //            if (context.Entry(ae).State != EntityState.Deleted)
            //                context.Entry(ae).State = EntityState.Modified;
            //            context.SaveChanges();
            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //    }
            //}


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
        private static List<Flight> GenerateFlights(Plane pl1, Plane pl2, Location location, Pilot pilot, StartType start)
        {
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
                    Betaler = pilot,
                    StartType = start,
                    Description = "Demo flight",
                    LastUpdatedBy = pilot.ToString()
                },
                new Flight
                {
                    Plane = pl2,
                    StartedFrom = location,
                    Pilot = pilot,
                    Betaler = pilot,
                    StartType = start,
                    Description = "Demo flight",
                    LastUpdatedBy = pilot.ToString()
                },
                new Flight
                {
                    Departure = DateTime.Now.AddHours(-2),
                    Plane = pl2,
                    StartedFrom = location,
                    Pilot = pilot,
                    Betaler = pilot,
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
                    Betaler = pilot,
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
                    Betaler = pilot,
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
                    Betaler = pilot,
                    StartType = start,
                    Description = "Demo flight",
                    LastUpdatedBy = pilot.ToString()
                }
            };
            return s;
        }
    }
}
