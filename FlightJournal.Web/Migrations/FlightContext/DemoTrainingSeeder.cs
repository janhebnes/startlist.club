using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Migrations.FlightContext
{
    /// <summary>Maps a <see cref="DemoTrainingScenario"/> plan to flights, applied exercises and annotations.</summary>
    public static class DemoTrainingSeeder
    {
        public const int BatchSize = 50;
        public const string SeedUser = "Demo seed";

        private static readonly string[] Notes =
        {
            "Demo: stable approach, keep airspeed up on final",
            "Demo: good lookout, work on rudder coordination in turns",
            "Demo: launch failure drill handled calmly",
            "Demo: circuit slightly wide, aim for the key point earlier",
            "Demo: smooth flare, hold off a little longer",
            "Demo: thermalling centred well, watch the bank angle"
        };

        /// <summary>
        /// Adds the planned training flights. Returns the number of flights created; 0 when there are no training programmes
        /// (a non-web seed such as the unit test database).
        /// </summary>
        public static int Apply(Models.FlightContext context, IReadOnlyList<PlannedTrainingFlight> plan, IDictionary<string, Pilot> actors,
            IDictionary<string, Location> locations, Plane twoSeater, Plane singleSeater, StartType start, int guidSeed = DemoTrainingScenario.DefaultSeed)
        {
            var program = context.TrainingPrograms.FirstOrDefault(p => p.ShortName == "SPL-S")
                          ?? context.TrainingPrograms.OrderBy(p => p.Training2ProgramId).FirstOrDefault();
            if (program == null)
            {
                Trace.TraceInformation("Demo training seed skipped: no training programmes");
                return 0;
            }

            // Everything the plan picks from, resolved once and in a stable order
            var exercises = program.Lessons.OrderBy(l => l.DisplayOrder).ThenBy(l => l.Training2LessonId)
                .SelectMany(l => l.Exercises.OrderBy(e => e.DisplayOrder).ThenBy(e => e.Training2ExerciseId).Select(e => new LessonExercise(l, e)))
                .ToList();
            if (!exercises.Any())
            {
                Trace.TraceInformation("Demo training seed skipped: the training programme has no exercises");
                return 0;
            }
            var practical = OrAll(exercises.Where(x => !x.Exercise.IsBriefingOnly).ToList(), exercises);
            var briefing = OrAll(exercises.Where(x => x.Exercise.IsBriefingOnly).ToList(), exercises);
            var gradings = context.Gradings.OrderBy(g => g.DisplayOrder).ThenBy(g => g.GradingId).ToList();
            var practicalGradings = OrAll(gradings.Where(g => g.AppliesToPracticalPartialExercises).ToList(), gradings);
            var briefingGradings = OrAll(gradings.Where(g => g.AppliesToBriefingOnlyPartialExercises).ToList(), gradings);
            var phaseComments = context.CommentaryTypes.Include(t => t.Commentaries).OrderBy(t => t.DisplayOrder).ThenBy(t => t.CommentaryTypeId).ToList()
                .SelectMany(t => t.Commentaries.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CommentaryId).Select(c => new PhaseComment(t, c)))
                .ToList();
            var manouvres = context.Manouvres.OrderBy(m => m.DisplayOrder).ThenBy(m => m.ManouvreId).ToList();

            var guids = new Random(guidSeed);
            var created = 0;
            var autoDetect = context.Configuration.AutoDetectChangesEnabled;
            try
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var planned in plan)
                {
                    var flight = AddFlight(context, planned, NextGuid(guids), actors, locations, twoSeater, singleSeater, start);
                    var instructor = actors[planned.Instructor];

                    var choices = planned.IsBriefingOnly ? briefing : practical;
                    foreach (var pick in planned.ExercisePicks)
                    {
                        var choice = choices[pick % choices.Count];
                        var applicable = choice.Exercise.IsBriefingOnly ? briefingGradings : practicalGradings;
                        context.AppliedExercises.Add(new AppliedExercise
                        {
                            FlightId = flight.FlightId,
                            Program = program,
                            Lesson = choice.Lesson,
                            Exercise = choice.Exercise,
                            Grading = applicable.Count == 0 ? null : applicable[planned.GradingPick % applicable.Count],
                            Instructor = instructor
                        });
                    }

                    if (planned.HasAnnotation)
                        AddAnnotation(context, planned, flight, phaseComments, manouvres);

                    if (++created % BatchSize == 0)
                        context.SaveChanges();
                }
                if (created % BatchSize != 0)
                    context.SaveChanges();
            }
            finally
            {
                context.Configuration.AutoDetectChangesEnabled = autoDetect;
            }

            Trace.TraceInformation($"Demo training seed: {created} training flights");
            return created;
        }

        private static Flight AddFlight(Models.FlightContext context, PlannedTrainingFlight planned, Guid flightId, IDictionary<string, Pilot> actors,
            IDictionary<string, Location> locations, Plane twoSeater, Plane singleSeater, StartType start)
        {
            var location = locations[planned.LocationKey];
            var departure = planned.Date.AddMinutes(planned.DepartureMinuteOfDay);
            var flight = new Flight
            {
                FlightId = flightId,
                Date = planned.Date // set before Departure/Landing, which follow the flight date
            };
            flight.Departure = departure;
            flight.Landing = departure.AddMinutes(planned.DurationMinutes);
            flight.PlaneId = planned.BackSeat == null ? singleSeater.PlaneId : twoSeater.PlaneId;
            flight.PilotId = actors[planned.FrontSeat].PilotId;
            flight.PilotBackseatId = planned.BackSeat == null ? (int?)null : actors[planned.BackSeat].PilotId;
            flight.BetalerId = actors[planned.Student].PilotId;
            flight.StartTypeId = start.StartTypeId;
            flight.StartedFromId = location.LocationId;
            flight.LandedOnId = location.LocationId;
            flight.HasTrainingData = true;
            flight.LastUpdated = departure;
            flight.LastUpdatedBy = SeedUser;
            flight.Deleted = planned.Deleted ? departure.AddDays(1) : (DateTime?)null;
            return context.Flights.Add(flight);
        }

        private static void AddAnnotation(Models.FlightContext context, PlannedTrainingFlight planned, Flight flight, IList<PhaseComment> phaseComments, IList<Manouvre> manouvres)
        {
            var annotation = new TrainingFlightAnnotation
            {
                FlightId = flight.FlightId,
                Note = planned.HasNote ? Notes[planned.NotePick % Notes.Length] : null,
                WindSpeed = planned.WindSpeedPick % 6 * 5,
                WindDirection = planned.WindDirectionPick % 8 * 45
            };
            if (manouvres.Count > 0)
                foreach (var pick in planned.ManouvrePicks)
                    annotation.Manouvres.Add(manouvres[pick % manouvres.Count]);
            if (planned.HasPhaseComments && phaseComments.Count > 0)
            {
                // Distinct pairs: the type/commentary pair is part of the row key
                foreach (var pair in planned.PhaseCommentPicks.Select(p => phaseComments[p % phaseComments.Count]).Distinct())
                {
                    annotation.TrainingFlightAnnotationCommentCommentTypes.Add(new TrainingFlightAnnotationCommentCommentType
                    {
                        TrainingFlightAnnotation = annotation,
                        Commentary = pair.Commentary,
                        CommentaryType = pair.Type,
                        CommentaryId = pair.Commentary.CommentaryId,
                        CommentaryTypeId = pair.Type.CommentaryTypeId
                    });
                }
            }
            context.TrainingFlightAnnotations.Add(annotation);
        }

        private static Guid NextGuid(Random random)
        {
            var bytes = new byte[16];
            random.NextBytes(bytes);
            return new Guid(bytes);
        }

        private static List<T> OrAll<T>(List<T> subset, List<T> all) => subset.Count > 0 ? subset : all;

        private sealed class LessonExercise
        {
            public LessonExercise(Training2Lesson lesson, Training2Exercise exercise)
            {
                Lesson = lesson;
                Exercise = exercise;
            }

            public Training2Lesson Lesson { get; }
            public Training2Exercise Exercise { get; }
        }

        private sealed class PhaseComment
        {
            public PhaseComment(CommentaryType type, Commentary commentary)
            {
                Type = type;
                Commentary = commentary;
            }

            public CommentaryType Type { get; }
            public Commentary Commentary { get; }
        }
    }
}
