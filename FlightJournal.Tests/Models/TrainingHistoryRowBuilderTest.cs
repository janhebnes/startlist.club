using System;
using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Models.Training;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    [TestClass]
    public class TrainingHistoryRowBuilderTest
    {
        private static readonly Guid FlightId = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly DateTime Date = new DateTime(2001, 3, 4);

        // Synthetic rows only
        private static FlightRowData Flight(int? pilotClubId = 1, int? backseatClubId = null, int? betalerClubId = 1, bool hasBackseat = false)
        {
            return new FlightRowData
            {
                FlightId = FlightId, Date = Date, Departure = Date.AddHours(10), Landing = Date.AddHours(10).AddMinutes(35), LandingCount = 2,
                PlaneCompetitionId = "T1", PlaneRegistration = "OY-TST", PilotName = "Test Student", PilotMemberId = "S1", PilotUnionId = "U1",
                PilotClubId = pilotClubId, HasBackseat = hasBackseat, BackseatUnionId = hasBackseat ? "U2" : null, BackseatClubId = backseatClubId,
                BetalerClubId = betalerClubId, StartedFromName = "Test Field"
            };
        }

        private static ExerciseRow Exercise(int id, int lessonId, string lessonName, int lessonOrder, string exerciseName, int exerciseOrder, int? grading = 3, string program = "TP", string instructor = "Test Instructor")
        {
            return new ExerciseRow
            {
                AppliedExerciseId = id, FlightId = FlightId, ProgramShortName = program, ProgramRequiresUnionIds = true,
                LessonId = lessonId, LessonName = lessonName, LessonDisplayOrder = lessonOrder, LessonCanDual = true, LessonCanSolo = true,
                ExerciseName = exerciseName, ExerciseDisplayOrder = exerciseOrder, GradingValue = grading,
                HasInstructor = instructor != null, InstructorName = instructor, InstructorClubShortName = instructor != null ? "TC" : null, InstructorUnionId = instructor != null ? "U9" : null
            };
        }

        private static TrainingFlightRow BuildOne(IEnumerable<ExerciseRow> exercises, IEnumerable<AnnotationRow> annotations = null,
            IEnumerable<PhaseCommentRow> comments = null, IEnumerable<ManoeuvreRow> manoeuvres = null, ClubScope club = null, FlightRowData flight = null)
        {
            var rows = TrainingHistoryRowBuilder.Build(
                new[] { flight ?? Flight() },
                exercises,
                annotations ?? Enumerable.Empty<AnnotationRow>(),
                comments ?? Enumerable.Empty<PhaseCommentRow>(),
                manoeuvres ?? Enumerable.Empty<ManoeuvreRow>(),
                club);
            Assert.AreEqual(1, rows.Count);
            return rows[0];
        }

        [TestMethod]
        public void FlightColumns_FormattedAsBefore()
        {
            var row = BuildOne(new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) });

            Assert.AreEqual(FlightId, row.FlightId);
            Assert.AreEqual(Date, row.Date);
            Assert.AreEqual(Date.AddHours(10).AddMinutes(35).ToString("yyyy-MM-dd HH:mm"), row.Timestamp);
            Assert.AreEqual("T1 (OY-TST)", row.Plane);
            Assert.AreEqual("Test Student (S1)", row.FrontSeatOccupant);
            Assert.AreEqual("Test Instructor (TC)", row.Instructor);
            Assert.AreEqual("Test Field", row.Airfield);
            Assert.AreEqual("00:35", row.Duration);
            Assert.AreEqual(2, row.LandingCount);
            Assert.AreEqual("TP", row.TrainingProgramName);
        }

        [TestMethod]
        public void PrimaryLesson_IsMostGraded()
        {
            var row = BuildOne(new[]
            {
                Exercise(1, 10, "Lesson A", 1, "Ex 1", 1),
                Exercise(2, 20, "Lesson B", 2, "Ex 2", 1),
                Exercise(3, 20, "Lesson B", 2, "Ex 3", 2)
            });

            Assert.AreEqual("Lesson B", row.PrimaryLessonName);
        }

        [TestMethod]
        public void PrimaryLesson_TieBrokenByHighestDisplayOrder()
        {
            var row = BuildOne(new[]
            {
                Exercise(1, 20, "Lesson B", 5, "Ex 2", 1),
                Exercise(2, 10, "Lesson A", 1, "Ex 1", 1)
            });

            Assert.AreEqual("Lesson B", row.PrimaryLessonName);
        }

        [TestMethod]
        public void ExerciseStrings_OrderedByLessonThenExerciseDisplayOrder()
        {
            var row = BuildOne(new[]
            {
                Exercise(1, 20, "Lesson B", 2, "Ex B2", 2, grading: 4),
                Exercise(2, 10, "Lesson A", 1, "Ex A1", 1, grading: null),
                Exercise(3, 20, "Lesson B", 2, "Ex B1", 1, grading: 5)
            });

            Assert.AreEqual("Lesson A-Ex A1 (), Lesson B-Ex B1 (5), Lesson B-Ex B2 (4)", row.ExercisesFull);
            Assert.AreEqual("Lesson A, Lesson B", row.ExercisesShort);
        }

        [TestMethod]
        public void ProgramNames_DistinctInFirstSeenOrder_InstructorFromFirstExerciseWithOne()
        {
            var row = BuildOne(new[]
            {
                Exercise(1, 10, "Lesson A", 1, "Ex 1", 1, program: "P2", instructor: null),
                Exercise(2, 10, "Lesson A", 1, "Ex 2", 2, program: "P1", instructor: "Test Second"),
                Exercise(3, 10, "Lesson A", 1, "Ex 3", 3, program: "P2", instructor: "Test Third")
            });

            Assert.AreEqual("P2, P1", row.TrainingProgramName);
            Assert.AreEqual("Test Second (TC)", row.Instructor);
        }

        [TestMethod]
        public void Notes_JoinedWithPhaseCommentsInPhaseOrder()
        {
            var row = BuildOne(
                new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) },
                new[] { new AnnotationRow { FlightId = FlightId, AnnotationId = 7, Note = "Test note" } },
                new[]
                {
                    new PhaseCommentRow { FlightId = FlightId, AnnotationId = 7, PhaseId = 2, PhaseCType = "Landing", PhaseDisplayOrder = 2, CommentaryId = 1, Comment = "Long" },
                    new PhaseCommentRow { FlightId = FlightId, AnnotationId = 7, PhaseId = 1, PhaseCType = "Launch", PhaseDisplayOrder = 1, CommentaryId = 3, Comment = "Late" },
                    new PhaseCommentRow { FlightId = FlightId, AnnotationId = 7, PhaseId = 1, PhaseCType = "Launch", PhaseDisplayOrder = 1, CommentaryId = 2, Comment = "Early" }
                });

            Assert.AreEqual("Test note - Launch: Early,Late - Landing: Long", row.Note);
        }

        [TestMethod]
        public void OnlyFirstAnnotationOfAFlightIsUsed()
        {
            var row = BuildOne(
                new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) },
                new[]
                {
                    new AnnotationRow { FlightId = FlightId, AnnotationId = 9, Note = "Later duplicate" },
                    new AnnotationRow { FlightId = FlightId, AnnotationId = 7, Note = "First" }
                },
                new[] { new PhaseCommentRow { FlightId = FlightId, AnnotationId = 9, PhaseId = 1, PhaseCType = "Launch", PhaseDisplayOrder = 1, CommentaryId = 1, Comment = "Ignored" } },
                new[] { new ManoeuvreRow { FlightId = FlightId, AnnotationId = 9, ManouvreId = 1, IconCssClass = "fa-x", ManouvreItem = "Ignored" } });

            Assert.AreEqual("First", row.Note);
            Assert.AreEqual("", row.Manouvres);
        }

        [TestMethod]
        public void Manoeuvres_RenderedAsIconHtml()
        {
            var row = BuildOne(
                new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) },
                new[] { new AnnotationRow { FlightId = FlightId, AnnotationId = 7 } },
                manoeuvres: new[]
                {
                    new ManoeuvreRow { FlightId = FlightId, AnnotationId = 7, ManouvreId = 2, IconCssClass = "fa-b", ManouvreItem = "Spin" },
                    new ManoeuvreRow { FlightId = FlightId, AnnotationId = 7, ManouvreId = 1, IconCssClass = "fa-a", ManouvreItem = "Stall" }
                });

            Assert.AreEqual("<i class='fa-a'></i>Stall, <i class='fa-b'></i>Spin", row.Manouvres);
        }

        [TestMethod]
        public void FlightWithoutAnnotation_HasEmptyNoteAndManoeuvres()
        {
            var row = BuildOne(new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) });

            Assert.AreEqual("", row.Note);
            Assert.AreEqual("", row.Manouvres);
        }

        [TestMethod]
        public void IsCurrentClubPilots_NoClubScope_True()
        {
            Assert.IsTrue(BuildOne(new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) }, flight: Flight(pilotClubId: 5, betalerClubId: 5)).IsCurrentClubPilots);
        }

        [TestMethod]
        public void IsCurrentClubPilots_MatchesPilotBackseatOrBetaler()
        {
            var club = new ClubScope { ClubId = 1, LocationId = 100 };
            var ex = new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) };

            Assert.IsTrue(BuildOne(ex, club: club, flight: Flight(pilotClubId: 1, betalerClubId: 5)).IsCurrentClubPilots, "pilot");
            Assert.IsTrue(BuildOne(ex, club: club, flight: Flight(pilotClubId: 5, backseatClubId: 1, betalerClubId: 5, hasBackseat: true)).IsCurrentClubPilots, "backseat");
            Assert.IsTrue(BuildOne(ex, club: club, flight: Flight(pilotClubId: 5, betalerClubId: 1)).IsCurrentClubPilots, "betaler");
            Assert.IsFalse(BuildOne(ex, club: club, flight: Flight(pilotClubId: 5, betalerClubId: 5)).IsCurrentClubPilots, "other club");
            Assert.IsTrue(BuildOne(ex, club: club, flight: Flight(pilotClubId: null, betalerClubId: null)).IsCurrentClubPilots, "no pilots");
        }

        [TestMethod]
        public void Validation_UsesRowFacts_WithoutEntities()
        {
            var flight = Flight();
            flight.PilotUnionId = null;

            var row = BuildOne(new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) }, flight: flight);

            Assert.IsFalse(row.PassesValidation);
            CollectionAssert.AreEqual(new[] { "Pilot is missing Union ID" }, row.Violations.ToArray());
        }

        [TestMethod]
        public void TimesNormalizedToFlightDate_AsTheEntitySettersDo()
        {
            var flight = Flight();
            flight.Departure = Date.AddDays(1).AddHours(9);
            flight.Landing = Date.AddDays(1).AddHours(9).AddMinutes(10);

            var row = BuildOne(new[] { Exercise(1, 10, "Lesson A", 1, "Ex 1", 1) }, flight: flight);

            Assert.AreEqual(Date.AddHours(9).AddMinutes(10).ToString("yyyy-MM-dd HH:mm"), row.Timestamp);
            Assert.AreEqual("00:10", row.Duration);
        }
    }
}
