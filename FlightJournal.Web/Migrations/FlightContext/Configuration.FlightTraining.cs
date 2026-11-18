using System.Collections.Generic;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Predefined;
using System;
using System.IO;
using System.Linq;
using FlightJournal.Web.Models.Training.Flight;
using Newtonsoft.Json;

namespace FlightJournal.Web.Migrations.FlightContext
{
    internal sealed partial class Configuration
    {
        internal class FlightTraining
        { 
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
                        IconCssClass = "fa fa-undo",
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
                var startCommentary = new CommentaryType { CType = "Start", Commentaries = new HashSet<Commentary>(), DisplayOrder = 0 };
                var flightCommentary = new CommentaryType { CType = "Flyvning", Commentaries = new HashSet<Commentary>(), DisplayOrder = 1 };
                var approachCommentary = new CommentaryType { CType = "Indflyvning", Commentaries = new HashSet<Commentary>(), DisplayOrder = 2 };
                var landingCommentary = new CommentaryType { CType = "Landing", Commentaries = new HashSet<Commentary>(), DisplayOrder = 3 };

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
                        GradingIdForExport = Guid.Empty,
                        Name = "Briefet",
                        IsOk = true,
                        DisplayOrder = 1,
                        Value = 3,
                        AppliesToBriefingOnlyPartialExercises = true,
                        AppliesToPracticalPartialExercises = false
                    },
                    new Grading
                    {
                        GradingIdForExport = Guid.Empty,
                        Name = "Kan kun udføres med hjælp fra instruktøren",
                        IsOk = false,
                        DisplayOrder = 1,
                        Value = 1,
                        AppliesToBriefingOnlyPartialExercises = false,
                        AppliesToPracticalPartialExercises = true
                    },
                    new Grading
                    {
                        GradingIdForExport = Guid.Empty,
                        Name = "Kan udføres med mundtlige korrektioner fra instruktøren",
                        IsOk = false,
                        DisplayOrder = 2,
                        Value = 2,
                        AppliesToBriefingOnlyPartialExercises = false,
                        AppliesToPracticalPartialExercises = true
                    },
                    new Grading
                    {
                        GradingIdForExport = Guid.Empty,
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

            public static void InitializeTrainingProgramsFromFileSystem(Models.FlightContext context, string importTrainingProgramJsonFilePath)
            {
                if (!File.Exists(importTrainingProgramJsonFilePath))
                    throw new FileNotFoundException(importTrainingProgramJsonFilePath);
                    
                using (var sr = new StreamReader(File.OpenRead(importTrainingProgramJsonFilePath)))
                using (var reader = new JsonTextReader(sr))
                {
                    var program = JsonConvert.DeserializeObject<Training2Program>(sr.ReadToEnd());
                    context.TrainingPrograms.Add(program);
                    context.SaveChanges();
                }
            }
        }
    }
}