using System;
using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Migrations.FlightContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Migrations
{
    [TestClass]
    public class DemoTrainingScenarioTest
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 26);
        private static IReadOnlyList<PlannedTrainingFlight> _plan;

        private static IReadOnlyList<PlannedTrainingFlight> Plan => _plan ?? (_plan = DemoTrainingScenario.Plan(Today));

        private static string Describe(PlannedTrainingFlight f)
        {
            return string.Join("|", f.Index, f.Date.ToString("yyyy-MM-dd"), f.DepartureMinuteOfDay, f.DurationMinutes, f.Student, f.Instructor, f.Seats,
                f.LocationKey, f.ExerciseCount, string.Join(",", f.ExercisePicks), f.GradingPick, f.HasAnnotation, f.HasNote, f.HasPhaseComments,
                f.NotePick, string.Join(",", f.PhaseCommentPicks), f.WindSpeedPick, f.WindDirectionPick, string.Join(",", f.ManouvrePicks), f.Deleted);
        }

        [TestMethod]
        public void Plan_IsDeterministic()
        {
            var first = DemoTrainingScenario.Plan(Today).Select(Describe).ToList();
            var second = DemoTrainingScenario.Plan(Today).Select(Describe).ToList();
            var otherSeed = DemoTrainingScenario.Plan(Today, seed: 1).Select(Describe).ToList();

            CollectionAssert.AreEqual(first, second);
            CollectionAssert.AreNotEqual(first, otherSeed);
            CollectionAssert.AreEqual(Enumerable.Range(0, first.Count).ToList(), Plan.Select(f => f.Index).ToList());
        }

        [TestMethod]
        public void Plan_DatesRelativeToToday_WithinWindow()
        {
            Assert.IsTrue(Plan.All(f => f.Date >= Today.AddDays(-730) && f.Date <= Today));
            Assert.IsTrue(Plan.All(f => f.Date.TimeOfDay == TimeSpan.Zero));

            // The window and the anchor days move with today. (Which other days fly depends on the calendar season,
            // so the full plan is not a pure shift.)
            var shifted = DemoTrainingScenario.Plan(Today.AddDays(45));
            Assert.IsTrue(shifted.All(f => f.Date >= Today.AddDays(45 - 730) && f.Date <= Today.AddDays(45)));
            foreach (var offset in new[] { 0, 1, 2, 3, 10, 366, 367 })
                Assert.IsTrue(shifted.Any(f => f.Date == Today.AddDays(45 - offset)), "today-" + offset);
        }

        [TestMethod]
        public void Plan_Volume_InRange()
        {
            Console.WriteLine($"Planned training flights: {Plan.Count}");
            Assert.IsTrue(Plan.Count >= 300 && Plan.Count <= 500, Plan.Count.ToString());
        }

        [TestMethod]
        public void Plan_CoversEditableWindowAndClampBoundary()
        {
            foreach (var offset in new[] { 0, 1, 2, 3, 366, 367 })
                Assert.IsTrue(Plan.Any(f => f.Date == Today.AddDays(-offset)), "today-" + offset);
        }

        [TestMethod]
        public void Plan_CoversEachOfLast24Months()
        {
            for (var back = 0; back < 24; back++)
            {
                var month = new DateTime(Today.Year, Today.Month, 1).AddMonths(-back);
                Assert.IsTrue(Plan.Any(f => f.Date.Year == month.Year && f.Date.Month == month.Month), month.ToString("yyyy-MM"));
            }
        }

        [TestMethod]
        public void Plan_EachStudentHasAllSeatPatterns()
        {
            foreach (var student in TrainingScenarioActors.Students)
                foreach (SeatPattern seats in Enum.GetValues(typeof(SeatPattern)))
                    Assert.IsTrue(Plan.Any(f => f.Student == student && f.Seats == seats && !f.Deleted), student + " " + seats);
        }

        [TestMethod]
        public void Plan_CrossClubPairsPresent()
        {
            Assert.IsTrue(Plan.Any(f => f.Student == TrainingScenarioActors.StudentOther && f.Instructor == TrainingScenarioActors.InstructorA));
            Assert.IsTrue(Plan.Any(f => f.Student == TrainingScenarioActors.StudentA && f.Instructor == TrainingScenarioActors.InstructorOther));
            Assert.IsTrue(Plan.All(f => f.Student != TrainingScenarioActors.StudentB || f.Instructor == TrainingScenarioActors.InstructorA));
        }

        [TestMethod]
        public void Plan_ExactlyOneDeleted()
        {
            var deleted = Plan.Where(f => f.Deleted).ToList();

            Assert.AreEqual(1, deleted.Count);
            Assert.AreEqual(TrainingScenarioActors.StudentA, deleted[0].Student);
            Assert.AreEqual(Today.AddDays(-10), deleted[0].Date);
            Assert.IsTrue(deleted[0].ExerciseCount >= 1 && deleted[0].HasNote && deleted[0].DurationMinutes > 0);
        }

        [TestMethod]
        public void Plan_HasNotesAndPhaseComments()
        {
            var annotated = Plan.Where(f => f.HasAnnotation).ToList();
            var annotationShare = (double)annotated.Count / Plan.Count;
            var noteShare = (double)annotated.Count(f => f.HasNote) / annotated.Count;
            var phaseShare = (double)annotated.Count(f => f.HasPhaseComments) / annotated.Count;

            Console.WriteLine($"Annotations {annotationShare:P0}, notes {noteShare:P0}, phase comments {phaseShare:P0}");
            Assert.IsTrue(Math.Abs(annotationShare - 0.5) <= 0.10, annotationShare.ToString());
            Assert.IsTrue(Math.Abs(noteShare - 0.6) <= 0.10, noteShare.ToString());
            Assert.IsTrue(Math.Abs(phaseShare - 0.6) <= 0.10, phaseShare.ToString());
            Assert.IsTrue(Plan.All(f => f.HasAnnotation || (!f.HasNote && !f.HasPhaseComments)));
            Assert.IsTrue(Plan.Where(f => f.HasPhaseComments).All(f => f.PhaseCommentPicks.Length >= 1 && f.PhaseCommentPicks.Length <= 3));
            Assert.IsTrue(Plan.All(f => f.ManouvrePicks.Length >= 1 && f.ManouvrePicks.Length <= 2));
        }

        [TestMethod]
        public void Plan_SoloFlightsHaveNoBackseat_AndAnInstructorActor()
        {
            var solo = Plan.Where(f => f.Seats == SeatPattern.StudentSolo).ToList();
            var share = (double)solo.Count / Plan.Count;

            Assert.IsTrue(Math.Abs(share - 0.15) <= 0.10, share.ToString());
            Assert.IsTrue(solo.All(f => f.Instructor == TrainingScenarioActors.InstructorA || f.Instructor == TrainingScenarioActors.InstructorOther));
            Assert.IsTrue(solo.All(f => f.BackSeat == null && f.FrontSeat == f.Student), "A solo flight has no back seat");
            var back = Plan.First(f => f.Seats == SeatPattern.InstructorFrontStudentBack);
            Assert.AreEqual(back.Instructor, back.FrontSeat);
            Assert.AreEqual(back.Student, back.BackSeat);
        }

        [TestMethod]
        public void Plan_BriefingOnlyFlightsHaveZeroDuration()
        {
            var briefing = Plan.Where(f => f.IsBriefingOnly).ToList();

            Assert.IsTrue(briefing.Count >= 1);
            Assert.IsTrue(briefing.All(f => f.DurationMinutes == 0 && f.ExerciseCount == 1));
            Assert.IsTrue(Plan.Where(f => !f.IsBriefingOnly).All(f => f.DurationMinutes >= 6 && f.DurationMinutes <= 55));
            Assert.IsTrue(Plan.All(f => f.DepartureMinuteOfDay >= 600 && f.DepartureMinuteOfDay + f.DurationMinutes <= 1020));
            Assert.IsTrue(Plan.All(f => f.ExerciseCount >= 1 && f.ExerciseCount <= 3 && f.ExercisePicks.Length == f.ExerciseCount));
        }
    }
}
