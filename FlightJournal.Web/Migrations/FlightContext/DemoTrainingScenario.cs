using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightJournal.Web.Migrations.FlightContext
{
    /// <summary>Actor keys used by the demo training plan; the seeder maps them to pilots.</summary>
    public static class TrainingScenarioActors
    {
        public const string StudentA = "SA";         // main club
        public const string StudentB = "SB";         // main club
        public const string StudentOther = "SC";     // other club
        public const string InstructorA = "IA";      // main club
        public const string InstructorOther = "IB";  // other club

        public const string MainLocation = "main";
        public const string OtherLocation = "other";

        public static readonly string[] Students = { StudentA, StudentB, StudentOther };
    }

    public enum SeatPattern { StudentFrontInstructorBack, InstructorFrontStudentBack, StudentSolo }

    public sealed class PlannedTrainingFlight
    {
        public int Index;                 // stable order
        public DateTime Date;             // local date, relative to today
        public int DepartureMinuteOfDay;  // 600..1020
        public int DurationMinutes;       // 0 for briefing-only, else 6..55
        public string Student;
        public string Instructor;         // also credited on solo flights (ground instructor)
        public SeatPattern Seats;
        public string LocationKey;        // "main" | "other"
        public int ExerciseCount;         // 1..3
        public int[] ExercisePicks;       // indexes into the programme's exercise list (resolved by the seeder)
        public int GradingPick;           // index into the applicable gradings
        public bool HasAnnotation, HasNote, HasPhaseComments;
        public int NotePick;
        public int[] PhaseCommentPicks;   // indexes into the allowed type/commentary pairs
        public int WindSpeedPick, WindDirectionPick;
        public int[] ManouvrePicks;       // 1-2 indexes into the manoeuvres
        public bool Deleted;

        public bool IsBriefingOnly => DurationMinutes == 0;

        /// <summary>The actor in the front seat (the flight's pilot).</summary>
        public string FrontSeat => Seats == SeatPattern.InstructorFrontStudentBack ? Instructor : Student;

        /// <summary>The actor in the back seat, or null on a solo flight.</summary>
        public string BackSeat => Seats == SeatPattern.StudentFrontInstructorBack ? Instructor
            : Seats == SeatPattern.InstructorFrontStudentBack ? Student
            : null;
    }

    /// <summary>
    /// A deterministic training scenario for empty dev/demo databases: about two years of flights for three students and
    /// two instructors, relative to today. Pure, so it can be tested without a database.
    /// </summary>
    public static class DemoTrainingScenario
    {
        public const int DefaultSeed = 4276;

        public static IReadOnlyList<PlannedTrainingFlight> Plan(DateTime today, int seed = DefaultSeed, int days = 730)
        {
            today = today.Date;
            var start = today.AddDays(-days);
            var random = new Random(seed);
            var deletedDay = today.AddDays(-10);

            // Days that must have flights: the editable window, the 366-day clamp boundary and the deleted flight's day
            var required = new HashSet<DateTime>(new[] { 0, 1, 2, 3, 366, 367, 10 }
                .Select(d => today.AddDays(-d))
                .Where(d => d >= start));

            var flyingDays = new SortedSet<DateTime>();
            for (var day = start; day <= today; day = day.AddDays(1))
            {
                var summer = day.Month >= 4 && day.Month <= 10;
                var chance = summer ? 2.0 / 7 : 1.0 / 21;
                if (random.NextDouble() < chance || required.Contains(day))
                    flyingDays.Add(day);
            }

            // At least one flying day in every calendar month the window touches (month and year navigation)
            for (var month = new DateTime(start.Year, start.Month, 1); month <= today; month = month.AddMonths(1))
            {
                var first = month < start ? start : month;
                var last = month.AddMonths(1).AddDays(-1) > today ? today : month.AddMonths(1).AddDays(-1);
                if (!flyingDays.Any(d => d >= first && d <= last))
                    flyingDays.Add(first.AddDays((last - first).Days / 2));
            }

            var flights = new List<PlannedTrainingFlight>();
            foreach (var day in flyingDays)
            {
                var count = 2 + random.Next(3);
                for (var slot = 0; slot < count; slot++)
                    flights.Add(NextFlight(random, day, slot));
                if (day == deletedDay)
                    flights.Add(DeletedFlight(random, day, count));
            }

            // Guarantee a briefing-only flight even if the dice never chose one
            if (!flights.Any(f => f.IsBriefingOnly))
                MakeBriefingOnly(flights[0]);

            var ordered = flights.OrderBy(f => f.Date).ThenBy(f => f.DepartureMinuteOfDay).ToList();
            for (var i = 0; i < ordered.Count; i++)
                ordered[i].Index = i;
            return ordered;
        }

        private static PlannedTrainingFlight NextFlight(Random random, DateTime day, int slot)
        {
            // Every flight consumes the same random values in the same order, whichever branches it takes
            var studentRoll = random.NextDouble();
            var crossClubRoll = random.NextDouble();
            var seatRoll = random.NextDouble();
            var briefingRoll = random.NextDouble();
            var departure = 600 + slot * 90 + random.Next(60);
            var duration = 6 + random.Next(50);

            var student = studentRoll < 0.4 ? TrainingScenarioActors.StudentA
                : studentRoll < 0.7 ? TrainingScenarioActors.StudentB
                : TrainingScenarioActors.StudentOther;
            string instructor;
            if (student == TrainingScenarioActors.StudentOther)
                instructor = crossClubRoll < 0.10 ? TrainingScenarioActors.InstructorA : TrainingScenarioActors.InstructorOther;
            else if (student == TrainingScenarioActors.StudentA)
                instructor = crossClubRoll < 0.05 ? TrainingScenarioActors.InstructorOther : TrainingScenarioActors.InstructorA;
            else
                instructor = TrainingScenarioActors.InstructorA;

            var flight = new PlannedTrainingFlight
            {
                Date = day,
                DepartureMinuteOfDay = departure,
                DurationMinutes = duration,
                Student = student,
                Instructor = instructor,
                Seats = seatRoll < 0.70 ? SeatPattern.StudentFrontInstructorBack
                    : seatRoll < 0.85 ? SeatPattern.InstructorFrontStudentBack
                    : SeatPattern.StudentSolo,
                LocationKey = instructor == TrainingScenarioActors.InstructorA ? TrainingScenarioActors.MainLocation : TrainingScenarioActors.OtherLocation,
            };
            AddContent(random, flight);
            if (briefingRoll < 0.03)
                MakeBriefingOnly(flight);
            return flight;
        }

        /// <summary>The single deleted flight: SA with IA about 10 days ago, with exercises and a note.</summary>
        private static PlannedTrainingFlight DeletedFlight(Random random, DateTime day, int slot)
        {
            var flight = new PlannedTrainingFlight
            {
                Date = day,
                DepartureMinuteOfDay = 600 + Math.Min(slot, 3) * 90 + random.Next(60),
                DurationMinutes = 6 + random.Next(50),
                Student = TrainingScenarioActors.StudentA,
                Instructor = TrainingScenarioActors.InstructorA,
                Seats = SeatPattern.StudentFrontInstructorBack,
                LocationKey = TrainingScenarioActors.MainLocation,
                Deleted = true
            };
            AddContent(random, flight);
            flight.HasAnnotation = true;
            flight.HasNote = true;
            return flight;
        }

        private static void AddContent(Random random, PlannedTrainingFlight flight)
        {
            flight.ExerciseCount = 1 + random.Next(3);
            flight.ExercisePicks = new[] { random.Next(1000), random.Next(1000), random.Next(1000) }.Take(flight.ExerciseCount).ToArray();
            flight.GradingPick = random.Next(100);
            flight.HasAnnotation = random.NextDouble() < 0.5;
            var noteRoll = random.NextDouble();
            var phaseRoll = random.NextDouble();
            flight.HasNote = flight.HasAnnotation && noteRoll < 0.6;
            flight.HasPhaseComments = flight.HasAnnotation && phaseRoll < 0.6;
            flight.NotePick = random.Next(100);
            var phaseCount = 1 + random.Next(3);
            flight.PhaseCommentPicks = new[] { random.Next(1000), random.Next(1000), random.Next(1000) }.Take(phaseCount).ToArray();
            flight.WindSpeedPick = random.Next(100);
            flight.WindDirectionPick = random.Next(100);
            var manouvreCount = 1 + random.Next(2);
            flight.ManouvrePicks = new[] { random.Next(1000), random.Next(1000) }.Take(manouvreCount).ToArray();
        }

        private static void MakeBriefingOnly(PlannedTrainingFlight flight)
        {
            flight.DurationMinutes = 0;
            flight.ExerciseCount = 1;
            flight.ExercisePicks = flight.ExercisePicks.Take(1).ToArray();
        }
    }
}
