using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models.Export
{
    /// <summary>
    /// Flat facts about a training flight that the export rules need; no entities, so no lazy loading.
    /// </summary>
    public sealed class TrainingFlightValidationInput
    {
        public double DurationMinutes { get; set; }
        public bool HasBackseat { get; set; }
        public bool PilotUnionIdMissing { get; set; }
        public bool BackseatUnionIdMissing { get; set; }
        public IReadOnlyList<TrainingExerciseValidationInput> Exercises { get; set; } = new List<TrainingExerciseValidationInput>();
    }

    public sealed class TrainingExerciseValidationInput
    {
        public bool RequiresUnionIds { get; set; }
        public bool InstructorUnionIdMissing { get; set; }
        public bool CanDual { get; set; }
        public bool CanSolo { get; set; }
    }

    /// <summary>
    /// Checks if a training flight conforms to the rules in its training program
    /// </summary>
    public class TrainingFlightExportValidator
    {
        private readonly TrainingFlightValidationInput _input;

        public List<string> Violations { get; } = new List<string>();
        public bool IsValid { get; private set; }

        /// <summary>Adapter for callers holding entities; maps them to <see cref="TrainingFlightValidationInput"/>.</summary>
        public TrainingFlightExportValidator(Flight flight, IEnumerable<AppliedExercise> aes)
            : this(ToInput(flight, aes))
        {
        }

        private TrainingFlightExportValidator(TrainingFlightValidationInput input)
        {
            _input = input;

            IsValid = true;
            if (!_input.Exercises.IsNullOrEmpty())
            {
                CheckUnionIdsIfRequired();
                CheckZeroDurationForNonFlyingExercises();
                CheckIfDurationCanBeUsedForDualOrSoloFlightType();
            }
        }

        /// <summary>The export rules over flat input (pure apart from message translation).</summary>
        public static TrainingFlightExportValidator Validate(TrainingFlightValidationInput input)
        {
            return new TrainingFlightExportValidator(input);
        }

        private static TrainingFlightValidationInput ToInput(Flight flight, IEnumerable<AppliedExercise> aes)
        {
            var exercises = aes?.ToList() ?? new List<AppliedExercise>();
            // Union ids are only inspected when a program requires them, so only then touch the related pilots
            var requiresUnionIds = exercises.Any(x => x.Program.RequireUnionIdsForExport);
            return new TrainingFlightValidationInput
            {
                DurationMinutes = flight.Duration.TotalMinutes,
                HasBackseat = flight.PilotBackseatId.HasValue,
                PilotUnionIdMissing = requiresUnionIds && flight.Pilot?.UnionId == null,
                BackseatUnionIdMissing = requiresUnionIds && flight.PilotBackseat is { UnionId: null },
                Exercises = exercises.Select(x => new TrainingExerciseValidationInput
                {
                    RequiresUnionIds = x.Program.RequireUnionIdsForExport,
                    InstructorUnionIdMissing = requiresUnionIds && x.Instructor is { UnionId: null },
                    CanDual = x.Lesson.CanHaveDualFlightDuration,
                    CanSolo = x.Lesson.CanHaveSoloFlightDuration
                }).ToList()
            };
        }

        /// <summary>
        /// Some DTOs (DSvU) identify pilots by the UnionId field
        /// </summary>
        private void CheckUnionIdsIfRequired()
        {
            if (_input.Exercises.Any(x => x.RequiresUnionIds))
            {
                if (_input.PilotUnionIdMissing)
                {
                    Violations.Add(_("Pilot is missing Union ID"));
                    IsValid = false;
                }
                if (_input.BackseatUnionIdMissing)
                {
                    Violations.Add(_("Back seat pilot is missing Union ID"));
                    IsValid = false;
                }
                if (_input.Exercises.Any(x => x.InstructorUnionIdMissing))
                {
                    Violations.Add(_("Instructor is missing Union ID"));
                    IsValid = false;
                }
            }
        }


        /// <summary>
        /// If a flight has gradings for only exercises (here: Lessons due to legacy naming) that are marked 'non-flying', check that the duration is zero.
        /// </summary>
        private void CheckZeroDurationForNonFlyingExercises()
        {
            if (_input.DurationMinutes > 0 && _input.Exercises.All(x => !(x.CanDual || x.CanSolo)))
            {
                IsValid = false;
                Violations.Add(_("Duration will not be used when only non-flying exercises are graded"));
            }
        }

        /// <summary>
        /// Not all exercises can log solo flight time
        /// Not all exercises can log dual flight time
        /// </summary>
        /// <returns></returns>
        private void CheckIfDurationCanBeUsedForDualOrSoloFlightType()
        {
            if (_input.DurationMinutes > 0)
            {
                if (!_input.HasBackseat && _input.Exercises.All(x => !x.CanSolo))
                {
                    IsValid = false;
                    Violations.Add(_("Duration will not be used for a solo flight when no solo exercises are graded"));
                }
                if (_input.HasBackseat && _input.Exercises.All(x => !x.CanDual))
                {
                    IsValid = false;
                    Violations.Add(_("Duration will not be used for a dual flight when no dual exercises are graded"));
                }
            }
        }




        private static string _(string resourceId)
        {
            return Internationalization.GetText(resourceId, Internationalization.LanguageCode);
        }

    }


}
