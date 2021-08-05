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
    /// Checks if a training flight conforms to the rules in its training program
    /// </summary>
    public class TrainingFlightExportValidator
    {
        private readonly Flight _flight;
        private readonly IEnumerable<AppliedExercise> _aes;

        public List<string> Violations { get; } = new List<string>();
        public bool IsValid { get; private set; }
        public TrainingFlightExportValidator(Flight flight, IEnumerable<AppliedExercise> aes)
        {
            _flight = flight;
            _aes = aes;


            IsValid = true;
            if (!_aes.IsNullOrEmpty())
            {
                CheckUnionIdsIfRequired();
                CheckZeroDurationForNonFlyingExercises();
                CheckIfDurationCanBeUsedForDualOrSoloFlightType();
            }
        }

        /// <summary>
        /// Some DTOs (DSvU) identify pilots by the UnionId field
        /// </summary>
        private void CheckUnionIdsIfRequired()
        {
            if (_aes.Any(x=>x.Program.RequireUnionIdsForExport))
            {
                if (_flight.Pilot?.UnionId == null)
                {
                    Violations.Add(_("Pilot is missing Union ID"));
                    IsValid = false;
                }
                if(_flight.PilotBackseat is { UnionId: null })
                {
                    Violations.Add(_("Back seat pilot is missing Union ID"));
                    IsValid = false;
                }
                if (_aes.Any(x=>x.Instructor is { UnionId: null }))
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
            if (_flight.Duration.TotalMinutes > 0 && _aes.All(x=>!(x.Lesson.CanHaveDualFlightDuration || x.Lesson.CanHaveSoloFlightDuration)))
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
            if (_flight.Duration.TotalMinutes > 0)
            {
                if (_flight.PilotBackseat == null && _aes.All(x => !x.Lesson.CanHaveSoloFlightDuration))
                {
                    IsValid = false;
                    Violations.Add(_("Duration will not be used for a solo flight when no solo exercises are graded"));
                }
                if (_flight.PilotBackseat != null && _aes.All(x => !x.Lesson.CanHaveDualFlightDuration))
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