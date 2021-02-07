using System.Collections.Generic;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Predefined;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

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

        internal static void InitializeCommentaries(Models.FlightContext context)
        {
            var startCommentary = new CommentaryType { CType = "Start", Commentaries = new List<Commentary>() };
            var flightCommentary = new CommentaryType { CType = "Flight", Commentaries = new List<Commentary>() };
            var approachCommentary = new CommentaryType { CType = "Approach", Commentaries = new List<Commentary>() };
            var landingCommentary = new CommentaryType { CType = "Landing", Commentaries = new List<Commentary>() };

            var commentOk = new Commentary { 
                            Comment = "&#x2713", 
                            CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
                            };
            startCommentary.Commentaries.Add(commentOk);
            flightCommentary.Commentaries.Add(commentOk);
            approachCommentary.Commentaries.Add(commentOk);
            landingCommentary.Commentaries.Add(commentOk);

            var commentAlmostOk = new Commentary
            {
                Comment = "(&#x2713)",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentAlmostOk);
            flightCommentary.Commentaries.Add(commentAlmostOk);
            approachCommentary.Commentaries.Add(commentAlmostOk);
            landingCommentary.Commentaries.Add(commentAlmostOk);

            var commentSkull = new Commentary
            {
                Comment = "&#x2620",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentSkull);
            flightCommentary.Commentaries.Add(commentSkull);
            approachCommentary.Commentaries.Add(commentSkull);
            landingCommentary.Commentaries.Add(commentSkull);

            var commentInstructorGuidance = new Commentary
            {
                Comment = "Instructor guidance needed",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentInstructorGuidance);
            flightCommentary.Commentaries.Add(commentInstructorGuidance);
            approachCommentary.Commentaries.Add(commentInstructorGuidance);
            landingCommentary.Commentaries.Add(commentInstructorGuidance);

            var commentInstructorTakeOver = new Commentary
            {
                Comment = "Instructor takeover needed",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentInstructorTakeOver);
            flightCommentary.Commentaries.Add(commentInstructorTakeOver);
            approachCommentary.Commentaries.Add(commentInstructorTakeOver);
            landingCommentary.Commentaries.Add(commentInstructorTakeOver);

            var commentUnstableDirection = new Commentary
            {
                Comment = "Unstable direction",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentUnstableDirection);
            flightCommentary.Commentaries.Add(commentUnstableDirection);
            approachCommentary.Commentaries.Add(commentUnstableDirection);
            landingCommentary.Commentaries.Add(commentUnstableDirection);

            var commentUnstableSpeed = new Commentary
            {
                Comment = "Unstable speed",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentUnstableSpeed);
            flightCommentary.Commentaries.Add(commentUnstableSpeed);
            approachCommentary.Commentaries.Add(commentUnstableSpeed);
            landingCommentary.Commentaries.Add(commentUnstableSpeed);

            var commentSpeedTooHigh = new Commentary
            {
                Comment = "Speed too high",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentSpeedTooHigh);
            flightCommentary.Commentaries.Add(commentSpeedTooHigh);
            approachCommentary.Commentaries.Add(commentSpeedTooHigh);
            landingCommentary.Commentaries.Add(commentSpeedTooHigh);

            var commentSpeedTooLow = new Commentary
            {
                Comment = "Speed too low",
                CommentaryTypes = new List<CommentaryType> { startCommentary, flightCommentary, approachCommentary, landingCommentary }
            };

            startCommentary.Commentaries.Add(commentSpeedTooLow);
            flightCommentary.Commentaries.Add(commentSpeedTooLow);
            approachCommentary.Commentaries.Add(commentSpeedTooLow);
            landingCommentary.Commentaries.Add(commentSpeedTooLow);

            var commentUnstablePosition = new Commentary
            {
                Comment = "Unstable position",
                CommentaryTypes = new List<CommentaryType> { flightCommentary, approachCommentary, landingCommentary }
            };

            flightCommentary.Commentaries.Add(commentUnstablePosition);
            approachCommentary.Commentaries.Add(commentUnstablePosition);
            landingCommentary.Commentaries.Add(commentUnstablePosition);

            var commentPosTooHigh = new Commentary
            {
                Comment = "Position too high",
                CommentaryTypes = new List<CommentaryType> { flightCommentary, approachCommentary, landingCommentary }
            };

            flightCommentary.Commentaries.Add(commentPosTooHigh);
            approachCommentary.Commentaries.Add(commentPosTooHigh);
            landingCommentary.Commentaries.Add(commentPosTooHigh);

            var commentPosTooLow = new Commentary
            {
                Comment = "Position too low",
                CommentaryTypes = new List<CommentaryType> { flightCommentary, approachCommentary, landingCommentary }
            };

            flightCommentary.Commentaries.Add(commentPosTooLow);
            approachCommentary.Commentaries.Add(commentPosTooLow);
            landingCommentary.Commentaries.Add(commentPosTooLow);

            var commentFlareTooHigh = new Commentary
            {
                Comment = "Flare too high",
                CommentaryTypes = new List<CommentaryType> { landingCommentary }
            };

            landingCommentary.Commentaries.Add(commentFlareTooHigh);

            var commentFlareTooLow = new Commentary
            {
                Comment = "Flare too low",
                CommentaryTypes = new List<CommentaryType> { landingCommentary }
            };

            landingCommentary.Commentaries.Add(commentFlareTooLow);
            foreach (var c in startCommentary.Commentaries)
            {
                c.AppliesToStartPhase = true;
            }
            foreach (var c in flightCommentary.Commentaries)
            {
                c.AppliesToFlightPhase= true;
            }
            foreach (var c in approachCommentary.Commentaries)
            {
                c.AppliesToApproachPhase = true;
            }
            foreach (var c in landingCommentary.Commentaries)
            {
                c.AppliesToLandingPhase = true;
            }
            context.Commentaries.Add(commentOk);
            context.Commentaries.Add(commentAlmostOk);
            context.Commentaries.Add(commentSkull);
            context.Commentaries.Add(commentInstructorGuidance);
            context.Commentaries.Add(commentInstructorTakeOver);
            context.Commentaries.Add(commentUnstableDirection);
            context.Commentaries.Add(commentUnstableSpeed);
            context.Commentaries.Add(commentSpeedTooHigh);
            context.Commentaries.Add(commentSpeedTooLow);
            context.Commentaries.Add(commentUnstablePosition);
            context.Commentaries.Add(commentPosTooHigh);
            context.Commentaries.Add(commentPosTooLow);
            context.Commentaries.Add(commentFlareTooHigh);
            context.Commentaries.Add(commentFlareTooLow);

            context.CommentaryTypes.Add(landingCommentary);
            context.CommentaryTypes.Add(startCommentary);
            context.CommentaryTypes.Add(approachCommentary);
            context.CommentaryTypes.Add(flightCommentary);
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

            // Training ui manouvre icons
/*            var iconLeftTurn = new ManouvreIcon { Icon = "fa fa-undo" };
            context.ManouvreIcons.Add(iconLeftTurn);
            var iconRightTurn = new ManouvreIcon { Icon = "fa fa-repeat" };
            context.ManouvreIcons.Add(iconRightTurn);
            var iconFigEight = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconFigEight);
            var iconBank30 = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconBank30);
            var iconBank45 = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconBank45);
            var iconBank60 = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconBank60);
            var iconAbortedLowAltitude = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconAbortedLowAltitude);
            var iconAbortedMidAltitude = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconAbortedMidAltitude);
            var iconAbortedHighAltitude = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconAbortedHighAltitude);
            var iconSTurn = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconSTurn);
            var iconLeftCircuit = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconLeftCircuit);
            var iconRightCircuit = new ManouvreIcon { Icon = "" };
            context.ManouvreIcons.Add(iconRightCircuit);*/

            var right90 = new Manouvre
            {
                ManouvreItem = "90",
                //ManouvreIcon = iconRightTurn,
                Description = "90 degree right turn",
                IconCssClass = "fa fa-repeat"
            };
            context.Manouvres.Add(right90);

            var left90 = new Manouvre
            {
                ManouvreItem = "90",
                //ManouvreIcon = iconLeftTurn,
                Description = "90 degree left turn",
                IconCssClass = "fa fa-undo"
            };
            context.Manouvres.Add(left90);

            var left180 = new Manouvre
            {
                ManouvreItem = "180",
                //ManouvreIcon = iconLeftTurn,
                Description = "180 degree left turn",
                IconCssClass = "fa fa-undo"
            };
            context.Manouvres.Add(left180);

            var right180 = new Manouvre
            {
                ManouvreItem = "180",
               // ManouvreIcon = iconRightTurn,
                Description = "180 degree right turn",
                IconCssClass = "fa fa-repeat"
            };
            context.Manouvres.Add(right180);

            var right360 = new Manouvre
            {
                ManouvreItem = "360",
                //ManouvreIcon = iconRightTurn,
                Description = "360 degree right turn",
                IconCssClass = "fa fa-repeat"
            };
            context.Manouvres.Add(right360);

            var left360 = new Manouvre
            {
                ManouvreItem = "360",
                //ManouvreIcon = iconLeftTurn,
                Description = "360 degree left turn",
                IconCssClass = "fa fa-repeat"
            };
            context.Manouvres.Add(left360);

            var FigureEight = new Manouvre
            {
                ManouvreItem = "&infin;",
                //ManouvreIcon = iconFigEight,
                Description = "Figure-eight"
            };
            context.Manouvres.Add(FigureEight);

            var Bank30 = new Manouvre
            {
                ManouvreItem = "&ang;30&deg;",
                //ManouvreIcon = iconBank30,
                Description = "30 degree bank"
            };
            context.Manouvres.Add(Bank30);

            var Bank45 = new Manouvre
            {
                ManouvreItem = "&ang;45&deg;",
                //ManouvreIcon = iconBank45,
                Description = "45 degree bank"
            };
            context.Manouvres.Add(Bank45);

            var Bank60 = new Manouvre
            {
                ManouvreItem = "&ang;60&deg;",
                //ManouvreIcon = iconBank60,
                Description = "60 degree bank"
            };
            context.Manouvres.Add(Bank60);

            var AbortedStartLowAlt = new Manouvre
            {
                ManouvreItem = "&#x21B7 Afb start lavt",
                //ManouvreIcon = iconAbortedLowAltitude
            };
            context.Manouvres.Add(AbortedStartLowAlt);

            var AbortedStartMedAlt = new Manouvre
            {
                ManouvreItem = "&#x21B7 Afb start mellem",
                //ManouvreIcon = iconAbortedMidAltitude
            };
            context.Manouvres.Add(AbortedStartMedAlt);

            var AbortedStartHighAlt = new Manouvre
            {
                ManouvreItem = "&#x21B7 Afb start højt",
                //ManouvreIcon = iconAbortedHighAltitude
            };
            context.Manouvres.Add(AbortedStartHighAlt);

            var Sturn = new Manouvre
            {
                ManouvreItem = "&#x219D S-drej",
                //ManouvreIcon = iconSTurn
            };
            context.Manouvres.Add(Sturn);

            var LandingLeftCircuit = new Manouvre
            {
                ManouvreItem = "&#x21B0 Landingsrunde",
                //ManouvreIcon = iconLeftCircuit
            };
            context.Manouvres.Add(LandingLeftCircuit);

            var LandingRightCircuit = new Manouvre
            {
                ManouvreItem = "&#x21B1 Landingsrunde",
                //ManouvreIcon = iconRightCircuit
            };
            context.Manouvres.Add(LandingRightCircuit);

        }

        internal static void InitializeWindDirections(Models.FlightContext context)
        {
            for (int v = 0; v < 360; v += 45)
            {
                var direction = new WindDirection { WindDirectionItem = v };
                context.WindDirections.Add(direction);
            }

        }

        internal static void InitializeWindSpeeds(Models.FlightContext context)
        {
            for (int v = 0; v < 30; v += 5)
            {
                var speed = new WindSpeed { WindSpeedItem = v };
                context.WindSpeeds.Add(speed);
            }
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
