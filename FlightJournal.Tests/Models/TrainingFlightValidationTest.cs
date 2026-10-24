using System;
using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Export;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    [TestClass]
    public class TrainingFlightValidationTest
    {
        // Without an HttpContext, translation returns the message id, so these are the untranslated messages
        private const string PilotMissingUnionId = "Pilot is missing Union ID";
        private const string BackseatMissingUnionId = "Back seat pilot is missing Union ID";
        private const string InstructorMissingUnionId = "Instructor is missing Union ID";
        private const string NonFlyingWithDuration = "Duration will not be used when only non-flying exercises are graded";
        private const string SoloWithoutSoloExercise = "Duration will not be used for a solo flight when no solo exercises are graded";
        private const string DualWithoutDualExercise = "Duration will not be used for a dual flight when no dual exercises are graded";

        private static TrainingExerciseValidationInput Exercise(bool requiresUnionIds = true, bool instructorUnionIdMissing = false, bool canDual = true, bool canSolo = true)
        {
            return new TrainingExerciseValidationInput { RequiresUnionIds = requiresUnionIds, InstructorUnionIdMissing = instructorUnionIdMissing, CanDual = canDual, CanSolo = canSolo };
        }

        private static TrainingFlightValidationInput Input(double minutes = 30, bool hasBackseat = true, bool pilotMissing = false, bool backseatMissing = false, params TrainingExerciseValidationInput[] exercises)
        {
            return new TrainingFlightValidationInput
            {
                DurationMinutes = minutes,
                HasBackseat = hasBackseat,
                PilotUnionIdMissing = pilotMissing,
                BackseatUnionIdMissing = backseatMissing,
                Exercises = exercises.Length == 0 ? new[] { Exercise() } : exercises
            };
        }

        [TestMethod]
        public void ValidFlight_IsValid_NoViolations()
        {
            var result = TrainingFlightExportValidator.Validate(Input());

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Violations.Count);
        }

        [TestMethod]
        public void NoExercises_IsValid()
        {
            var result = TrainingFlightExportValidator.Validate(new TrainingFlightValidationInput { DurationMinutes = 30, PilotUnionIdMissing = true });

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void UnionIdsRequired_MissingOnPilotBackseatAndInstructor_AllReported()
        {
            var result = TrainingFlightExportValidator.Validate(Input(pilotMissing: true, backseatMissing: true,
                exercises: Exercise(instructorUnionIdMissing: true)));

            Assert.IsFalse(result.IsValid);
            CollectionAssert.AreEqual(new[] { PilotMissingUnionId, BackseatMissingUnionId, InstructorMissingUnionId }, result.Violations);
        }

        [TestMethod]
        public void UnionIdsNotRequired_MissingIdsIgnored()
        {
            var result = TrainingFlightExportValidator.Validate(Input(pilotMissing: true, backseatMissing: true,
                exercises: Exercise(requiresUnionIds: false, instructorUnionIdMissing: true)));

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void NonFlyingExercisesOnly_WithDuration_Invalid()
        {
            var result = TrainingFlightExportValidator.Validate(Input(exercises: Exercise(canDual: false, canSolo: false)));

            Assert.IsFalse(result.IsValid);
            CollectionAssert.Contains(result.Violations, NonFlyingWithDuration);
        }

        [TestMethod]
        public void NonFlyingExercisesOnly_ZeroDuration_Valid()
        {
            var result = TrainingFlightExportValidator.Validate(Input(minutes: 0, exercises: Exercise(canDual: false, canSolo: false)));

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void SoloFlight_WithoutSoloExercise_Invalid()
        {
            var result = TrainingFlightExportValidator.Validate(Input(hasBackseat: false, exercises: Exercise(canSolo: false)));

            Assert.IsFalse(result.IsValid);
            CollectionAssert.AreEqual(new[] { SoloWithoutSoloExercise }, result.Violations);
        }

        [TestMethod]
        public void DualFlight_WithoutDualExercise_Invalid()
        {
            var result = TrainingFlightExportValidator.Validate(Input(hasBackseat: true, exercises: Exercise(canDual: false)));

            Assert.IsFalse(result.IsValid);
            CollectionAssert.AreEqual(new[] { DualWithoutDualExercise }, result.Violations);
        }

        [TestMethod]
        public void EntityAdapter_GivesSameResultAsValidateOnMappedInput()
        {
            // Synthetic entities only
            var program = new Training2Program("TP", "Test program", "") { RequireUnionIdsForExport = true };
            var soloOnlyLesson = new Training2Lesson("Test solo lesson", "purpose") { CanHaveDualFlightDuration = false, CanHaveSoloFlightDuration = true };
            var flight = new Flight { Date = new DateTime(2001, 1, 1) };
            flight.Departure = flight.Date.AddHours(10);
            flight.Landing = flight.Date.AddHours(10).AddMinutes(20);
            flight.Pilot = new Pilot { Name = "Test student", UnionId = null };
            flight.PilotBackseatId = 2;
            flight.PilotBackseat = new Pilot { Name = "Test instructor", UnionId = "U2" };
            var aes = new List<AppliedExercise>
            {
                new AppliedExercise { Program = program, Lesson = soloOnlyLesson, Instructor = new Pilot { Name = "Test instructor", UnionId = null } }
            };

            var adapter = new TrainingFlightExportValidator(flight, aes);
            var direct = TrainingFlightExportValidator.Validate(Input(minutes: 20, hasBackseat: true, pilotMissing: true, backseatMissing: false,
                exercises: Exercise(requiresUnionIds: true, instructorUnionIdMissing: true, canDual: false, canSolo: true)));

            Assert.AreEqual(direct.IsValid, adapter.IsValid);
            CollectionAssert.AreEqual(direct.Violations, adapter.Violations);
            CollectionAssert.AreEqual(new[] { PilotMissingUnionId, InstructorMissingUnionId, DualWithoutDualExercise }, adapter.Violations);
        }
    }
}
