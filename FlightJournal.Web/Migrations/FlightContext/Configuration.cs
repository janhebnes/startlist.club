using System.Collections.Generic;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Predefined;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using FlightJournal.Web.Models.Training.Flight;
using Newtonsoft.Json;

namespace FlightJournal.Web.Migrations.FlightContext
{

    internal sealed class Configuration : DbMigrationsConfiguration<FlightJournal.Web.Models.FlightContext>
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
            var club = new Club() { ClubId = 38, ShortName = "ØSF", Name = "Øst-Sjællands Flyveklub", Location = location, Website = "http://flyveklubben.dk" };
            context.Clubs.Add(club);
            var club2 = new Club() { ClubId = 99, ShortName = "AASVK", Name = "Århus Svæveflyveklub", Location = location2, Website = "http://www.aasvk.dk" };
            context.Clubs.Add(club2);
            var club3 = new Club() { ClubId = 199, ShortName = "MSF", Name = "Midtsjællands Svæveflyveklub", Location = location3, Website = "http://slaglille.dk" };
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

            context.SaveChanges();
        }

        internal static void InitializeManouvres(Models.FlightContext context)
        {
            context.Manouvres.AddRange(new[]
            {
                new Manouvre
                {
                    ManouvreItem = "90",
                    Description = "90 graders højredrej",
                    IconCssClass = "fa fa-repeat",
                    DisplayOrder = 1
                },
                new Manouvre
                {
                    ManouvreItem = "90",
                    Description = "90 grades venstredrej",
                    IconCssClass = "fa fa-undo",
                    DisplayOrder = 2
                },
                new Manouvre
                {
                    ManouvreItem = "180",
                    Description = "180 graders venstredrej",
                    IconCssClass = "fa fa-undo",
                    DisplayOrder = 3
                },
                new Manouvre
                {
                    ManouvreItem = "180",
                    Description = "180 graders højredrej",
                    IconCssClass = "fa fa-repeat",
                    DisplayOrder = 4
                },
                new Manouvre
                {
                    ManouvreItem = "360",
                    Description = "360 fuldkurve til højre",
                    IconCssClass = "fa fa-repeat",
                    DisplayOrder = 5
                },
                new Manouvre
                {
                    ManouvreItem = "360",
                    Description = "360 fuldkurve til venstre",
                    IconCssClass = "fa fa-repeat",
                    DisplayOrder = 6
                },
                new Manouvre
                {
                    ManouvreItem = "&infin;",
                    Description = "Ottetal",
                    DisplayOrder = 7
                },
                new Manouvre
                {
                    ManouvreItem = "&ang;30&deg;",
                    Description = "30 graders krængning",
                    DisplayOrder = 8
                },
                new Manouvre
                {
                    ManouvreItem = "&ang;45&deg;",
                    Description = "45 graders krængning",
                    DisplayOrder = 9
                },
                new Manouvre
                {
                    ManouvreItem = "&ang;60&deg;",
                    Description = "60 graders krængning",
                    DisplayOrder = 10
                },
                new Manouvre
                {
                    ManouvreItem = "&#x21B7 Afb start L",
                    Description = "Afbrudt start i lav højde",
                    DisplayOrder = 11
                },
                new Manouvre
                {
                    ManouvreItem = "&#x21B7 Afb start M",
                    Description = "Afbrudt start i mellem højde",
                    DisplayOrder = 12
                },
                new Manouvre
                {
                    ManouvreItem = "&#x21B7 Afb start H",
                    Description = "Afbrudt start i stor højde",
                    DisplayOrder = 13
                },
                new Manouvre
                {
                    ManouvreItem = "&#x219D S-drej",
                    DisplayOrder = 14
                },
                new Manouvre
                {
                    ManouvreItem = "&#x21B0 Landingsrunde V",
                    DisplayOrder = 15
                },
                new Manouvre
                {
                    ManouvreItem = "&#x21B1 Landingsrunde H",
                    DisplayOrder = 16
                }
            });

            context.SaveChanges();
        }

        internal static void InitializeWindDirections(Models.FlightContext context)
        {
            for (int v = 0; v < 360; v += 45)
            {
                var direction = new WindDirection { WindDirectionItem = v };
                context.WindDirections.Add(direction);
            }

            context.SaveChanges();
        }

        internal static void InitializeWindSpeeds(Models.FlightContext context)
        {
            for (int v = 0; v < 30; v += 5)
            {
                var speed = new WindSpeed { WindSpeedItem = v };
                context.WindSpeeds.Add(speed);
            }
            context.SaveChanges();
        }
        internal static void InitializeCommentaries(Models.FlightContext context)
        {
            var startCommentary = new CommentaryType {CType = "Start", Commentaries = new HashSet<Commentary>(), DisplayOrder = 0};
            var flightCommentary = new CommentaryType {CType = "Flyvning", Commentaries = new HashSet<Commentary>(), DisplayOrder = 1};
            var approachCommentary = new CommentaryType {CType = "Indflyvning", Commentaries = new HashSet<Commentary>(), DisplayOrder = 2};
            var landingCommentary = new CommentaryType {CType = "Landing", Commentaries = new HashSet<Commentary>(), DisplayOrder = 3};

            context.Commentaries.AddRange(new[]
            {
                new Commentary
                {
                    Comment = "&#x2713",
                    DisplayOrder = 1,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary},
                    IsOk = true
                },

                new Commentary
                {
                    Comment = "(&#x2713)",
                    DisplayOrder = 2,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "&#x2620",
                    DisplayOrder = 3,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Instruktørvejledning nødvendig",
                    DisplayOrder = 4,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Instruktørindgriben nødvendig",
                    DisplayOrder = 5,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Retning ustabil",
                    DisplayOrder = 6,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Fart ustabil",
                    DisplayOrder = 7,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Fart for høj",
                    DisplayOrder = 8,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Fart for lav",
                    DisplayOrder = 9,
                    CommentaryTypes = new HashSet<CommentaryType> {startCommentary, flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Position ustabil",
                    DisplayOrder = 10,
                    CommentaryTypes = new HashSet<CommentaryType> {flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Position for høj",
                    DisplayOrder = 11,
                    CommentaryTypes = new HashSet<CommentaryType> {flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Position for lav",
                    DisplayOrder = 12,
                    CommentaryTypes = new HashSet<CommentaryType> {flightCommentary, approachCommentary, landingCommentary}
                },
                new Commentary
                {
                    Comment = "Udfladning for høj",
                    DisplayOrder = 13,
                    CommentaryTypes = new HashSet<CommentaryType> {landingCommentary}
                },
                new Commentary
                {
                    Comment = "Udfladning for lav",
                    DisplayOrder = 14,
                    CommentaryTypes = new HashSet<CommentaryType> {landingCommentary}
                }
            });

            context.CommentaryTypes.Add(landingCommentary);
            context.CommentaryTypes.Add(startCommentary);
            context.CommentaryTypes.Add(approachCommentary);
            context.CommentaryTypes.Add(flightCommentary);

            context.SaveChanges();
        }

        internal static void InitializeGradings(Models.FlightContext context)
        {
            context.Gradings.AddRange(new[]
            {
                new Grading
                {
                    GradingIdForExport = Guid.NewGuid(),
                    Name = "Briefet",
                    IsOk = true,
                    DisplayOrder = 1,
                    Value = 3,
                    AppliesToBriefingOnlyPartialExercises = true,
                    AppliesToPracticalPartialExercises = false
                },
                new Grading
                {
                    GradingIdForExport = Guid.NewGuid(),
                    Name = "Kan kun udføres med hjælp fra instruktøren",
                    IsOk = false,
                    DisplayOrder = 1,
                    Value = 1,
                    AppliesToBriefingOnlyPartialExercises = false,
                    AppliesToPracticalPartialExercises = true
                },
                new Grading
                {
                    GradingIdForExport = Guid.NewGuid(),
                    Name = "Kan udføres med mundtlige korrektioner fra instruktøren",
                    IsOk = false,
                    DisplayOrder = 2,
                    Value = 2,
                    AppliesToBriefingOnlyPartialExercises = false,
                    AppliesToPracticalPartialExercises = true
                },
                new Grading
                {
                    GradingIdForExport = Guid.NewGuid(),
                    Name = "Udføres selvstændigt og tilfredsstillende",
                    IsOk = true,
                    DisplayOrder = 3,
                    Value = 3,
                    AppliesToBriefingOnlyPartialExercises = false,
                    AppliesToPracticalPartialExercises = true
                }
            });
            context.SaveChanges();
        }

        protected override void Seed(FlightJournal.Web.Models.FlightContext context)
        {
            //  This method will be called after migrating to the latest version.

            var forceTrainingProgramRecreation = false;

            if (forceTrainingProgramRecreation
                || !context.TrainingPrograms.Any()
                )
            {
                TrainingProgramCatalogue.InitializeTrainingPrograms(context);
            }

            //  Only seed if the database is empty
            if (!context.StartTypes.Any()
                && (!context.Clubs.Any())
                && (!context.Pilots.Any())
                && (!context.Planes.Any()))
            {
                InitializeDemoFlights(context);
            }


            if (forceTrainingProgramRecreation || !context.Manouvres.Any())
            {
                context.Manouvres.RemoveRange(context.Manouvres);
                InitializeManouvres(context);
            }


            if (forceTrainingProgramRecreation || !context.WindSpeeds.Any())
            {
                context.WindSpeeds.RemoveRange(context.WindSpeeds);
                InitializeWindSpeeds(context);
            }

            if (forceTrainingProgramRecreation || !context.WindDirections.Any())
            {
                context.WindDirections.RemoveRange(context.WindDirections);
                InitializeWindDirections(context);
            }

            if (forceTrainingProgramRecreation || !context.Commentaries.Any())
            {
                context.Commentaries.RemoveRange(context.Commentaries);
                context.CommentaryTypes.RemoveRange(context.CommentaryTypes);
                InitializeCommentaries(context);
            }

            if (forceTrainingProgramRecreation || !context.Gradings.Any())
            {
                context.Gradings.RemoveRange(context.Gradings);
                InitializeGradings(context);
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
