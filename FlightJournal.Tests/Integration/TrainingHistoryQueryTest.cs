using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Linq;
using System.Transactions;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Integration
{
    /// <summary>
    /// Runs against the LocalDB development database. Every test works inside a TransactionScope that is
    /// never completed, so all synthetic data is rolled back.
    /// </summary>
    [TestClass]
    public class TrainingHistoryQueryTest
    {
        // A year with no seeded demo flights, so ranges inside it only see this test's synthetic data
        private static readonly DateTime Year = new DateTime(2001, 1, 1);

        private TransactionScope _scope;
        private FlightContext _db;
        private CommandCounter _counter;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Same database as the RouteFilters tests
            var folder = Directory.GetDirectoryRoot(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\FlightJournal.Web\\App_Data"));
            AppDomain.CurrentDomain.SetData("DataDirectory", folder);
            // Run migrations and seeding outside any test transaction
            using (var db = new FlightContext())
                db.Database.Initialize(false);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(5) });
            _db = new FlightContext();
            _counter = new CommandCounter();
            DbInterception.Add(_counter);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DbInterception.Remove(_counter);
            _db.Dispose();
            _scope.Dispose(); // never completed: rolls back
        }

        /// <summary>Reproduces the current controller pattern: id list, IN (...) query, per-flight annotation lookup and lazy loads.</summary>
        private int CountOldPatternCommands(DateTime from, DateTime to)
        {
            _counter.Reset();
            var ids = DbHelper.IdsOfTrainingFlights(_db);
            var flights = _db.Flights.Where(x => x.Deleted == null && ids.Contains(x.FlightId) && x.Date >= from && x.Date <= to).ToList();
            foreach (var f in flights)
            {
                var annotation = _db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);
                var touched = f.Plane.Registration + f.Pilot.Name + f.StartedFrom.Name + (annotation?.Manouvres.Count ?? 0);
                Assert.IsNotNull(touched);
            }
            return _counter.Count;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Baseline_OldPattern_QueryCountGrowsWithFlights()
        {
            var data = SyntheticTrainingData.Create(_db);
            data.AddTrainingFlights(_db, 5, Year.AddDays(10));
            Detach();
            var countAt5 = CountOldPatternCommands(Year, Year.AddDays(364));

            Detach(); // drop the copies the lazy loads materialized before re-attaching the originals
            data.Reattach(_db);
            data.AddTrainingFlights(_db, 35, Year.AddDays(20));
            Detach();
            var countAt40 = CountOldPatternCommands(Year, Year.AddDays(364));

            Console.WriteLine($"Old pattern SQL commands: N=5 -> {countAt5}, N=40 -> {countAt40}");
            Assert.IsTrue(countAt40 > countAt5, "The old pattern's command count grows with the number of flights");
        }

        private IReadOnlyList<TrainingFlightRow> Load(TrainingHistoryFilter filter, ClubScope currentClub = null)
        {
            Detach();
            _counter.Reset();
            return new TrainingHistoryQuery(_db).Load(filter, currentClub);
        }

        private static TrainingHistoryFilter InYear(TrainingHistoryFilter filter = null)
        {
            filter = filter ?? new TrainingHistoryFilter();
            filter.From = filter.From ?? Year;
            filter.To = filter.To ?? Year.AddDays(364);
            return filter;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void QueryCount_IsConstant_InNumberOfFlights()
        {
            var data = SyntheticTrainingData.Create(_db);
            data.AddTrainingFlights(_db, 5, Year.AddDays(10));
            var rowsAt5 = Load(InYear());
            var countAt5 = _counter.Count;

            data.Reattach(_db);
            data.AddTrainingFlights(_db, 35, Year.AddDays(20));
            var rowsAt40 = Load(InYear());
            var countAt40 = _counter.Count;

            Console.WriteLine($"TrainingHistoryQuery SQL commands: N=5 -> {countAt5}, N=40 -> {countAt40}");
            Assert.AreEqual(5, rowsAt5.Count);
            Assert.AreEqual(40, rowsAt40.Count);
            Assert.AreEqual(countAt5, countAt40, "Command count must not depend on the number of flights");
            Assert.IsTrue(countAt5 <= 6, $"Expected at most 6 commands, got {countAt5}");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_ProjectsRowContent()
        {
            var data = SyntheticTrainingData.Create(_db);
            data.AddTrainingFlights(_db, 1, Year.AddDays(10));

            var row = Load(InYear()).Single();

            Assert.AreEqual($"{data.Plane.CompetitionId} ({data.Plane.Registration})", row.Plane);
            Assert.AreEqual($"{data.Student.Name} ({data.Student.MemberId})", row.FrontSeatOccupant);
            Assert.AreEqual($"{data.Instructor.Name} ({data.HomeClub.ShortName})", row.Instructor);
            Assert.AreEqual(data.Home.Name, row.Airfield);
            Assert.AreEqual("00:25", row.Duration);
            Assert.AreEqual(data.Program.ShortName, row.TrainingProgramName);
            Assert.AreEqual(data.LessonB.Name, row.PrimaryLessonName, "Equal counts: the highest display order wins");
            Assert.AreEqual($"{data.LessonA.Name}-{data.Exercise1.Name} (3), {data.LessonB.Name}-{data.Exercise2.Name} (3)", row.ExercisesFull);
            Assert.AreEqual($"{data.LessonA.Name}, {data.LessonB.Name}", row.ExercisesShort);
            Assert.AreEqual($"Test note - {data.Phase.CType}: {data.Comment.Comment}", row.Note);
            Assert.AreEqual($"<i class='fa-test'></i>{data.Manoeuvre.ManouvreItem}", row.Manouvres);
            Assert.IsTrue(row.PassesValidation, string.Join("; ", row.Violations));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_FiltersByClubInSql()
        {
            var data = SyntheticTrainingData.Create(_db);
            var home = data.AddTrainingFlights(_db, 2, Year.AddDays(10)).Select(f => f.FlightId).ToList();
            var away = data.AddFlight(_db, Year.AddDays(10), data.Away, data.AwayPilot, null);
            data.AddExercise(_db, away, data.Program, data.LessonA, data.Exercise1);
            _db.SaveChanges();

            var rows = Load(InYear(new TrainingHistoryFilter { Club = new ClubScope { ClubId = data.HomeClub.ClubId, LocationId = data.Home.LocationId } }));

            CollectionAssert.AreEquivalent(home, rows.Select(r => r.FlightId).ToList());
            var inLists = _counter.Commands.Where(c => c.Contains(" IN (")).ToList();
            Assert.AreEqual(0, inLists.Count, "No literal IN lists expected:\n" + string.Join("\n---\n", inLists));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_FiltersByProgramAndLesson()
        {
            var data = SyntheticTrainingData.Create(_db);
            var programFlight = data.AddTrainingFlights(_db, 1, Year.AddDays(10)).Single();
            var otherProgramFlight = data.AddTrainingFlights(_db, 1, Year.AddDays(11), program: data.OtherProgram).Single();
            var lessonBOnly = data.AddFlight(_db, Year.AddDays(12), data.Home, data.Student, data.Instructor);
            data.AddExercise(_db, lessonBOnly, data.Program, data.LessonB, data.Exercise2);
            _db.SaveChanges();

            var byProgram = Load(InYear(new TrainingHistoryFilter { ProgramId = data.OtherProgram.Training2ProgramId }));
            var byLesson = Load(InYear(new TrainingHistoryFilter { LessonId = data.LessonA.Training2LessonId }));

            CollectionAssert.AreEqual(new[] { otherProgramFlight.FlightId }, byProgram.Select(r => r.FlightId).ToArray());
            CollectionAssert.AreEquivalent(new[] { programFlight.FlightId, otherProgramFlight.FlightId }, byLesson.Select(r => r.FlightId).ToArray());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_FiltersByPilot_FrontOrBackSeat()
        {
            var data = SyntheticTrainingData.Create(_db);
            var studentWithInstructor = data.AddTrainingFlights(_db, 1, Year.AddDays(10)).Single();
            var awayWithInstructor = data.AddFlight(_db, Year.AddDays(11), data.Home, data.AwayPilot, data.Instructor);
            data.AddExercise(_db, awayWithInstructor, data.Program, data.LessonA, data.Exercise1);
            var awaySolo = data.AddFlight(_db, Year.AddDays(12), data.Home, data.AwayPilot, null);
            data.AddExercise(_db, awaySolo, data.Program, data.LessonA, data.Exercise1);
            _db.SaveChanges();

            var instructor = Load(InYear(new TrainingHistoryFilter { PilotId = data.Instructor.PilotId }));
            var student = Load(InYear(new TrainingHistoryFilter { PilotId = data.Student.PilotId }));

            CollectionAssert.AreEquivalent(new[] { studentWithInstructor.FlightId, awayWithInstructor.FlightId }, instructor.Select(r => r.FlightId).ToArray());
            CollectionAssert.AreEqual(new[] { studentWithInstructor.FlightId }, student.Select(r => r.FlightId).ToArray());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_DateRangeInclusive()
        {
            var data = SyntheticTrainingData.Create(_db);
            var from = Year.AddDays(100);
            var to = Year.AddDays(105);
            data.AddTrainingFlights(_db, 1, from.AddDays(-1));
            var first = data.AddTrainingFlights(_db, 1, from).Single();
            var last = data.AddTrainingFlights(_db, 1, to).Single();
            data.AddTrainingFlights(_db, 1, to.AddDays(1));

            var rows = Load(new TrainingHistoryFilter { From = from, To = to });

            CollectionAssert.AreEquivalent(new[] { first.FlightId, last.FlightId }, rows.Select(r => r.FlightId).ToArray());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_ExcludesDeleted()
        {
            var data = SyntheticTrainingData.Create(_db);
            var flights = data.AddTrainingFlights(_db, 2, Year.AddDays(10));
            flights[1].Deleted = Year.AddDays(11);
            _db.SaveChanges();

            var rows = Load(InYear());

            CollectionAssert.AreEqual(new[] { flights[0].FlightId }, rows.Select(r => r.FlightId).ToArray());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_IgnoresHasTrainingDataFlag()
        {
            var data = SyntheticTrainingData.Create(_db);
            var withExercises = data.AddTrainingFlights(_db, 1, Year.AddDays(10)).Single();
            withExercises.HasTrainingData = false;
            var flaggedWithoutExercises = data.AddFlight(_db, Year.AddDays(10), data.Home, data.Student, data.Instructor);
            flaggedWithoutExercises.HasTrainingData = true;
            _db.SaveChanges();

            var rows = Load(InYear());

            CollectionAssert.AreEqual(new[] { withExercises.FlightId }, rows.Select(r => r.FlightId).ToArray());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void StatementText_DoesNotGrowWithHistory()
        {
            var data = SyntheticTrainingData.Create(_db);
            data.AddTrainingFlights(_db, 1, Year.AddDays(10));
            data.AddTrainingFlights(_db, 10, Year.AddYears(1).AddDays(10));
            Load(InYear());
            var textWith10 = _counter.Commands.First();

            data.Reattach(_db);
            data.AddTrainingFlights(_db, 190, Year.AddYears(1).AddDays(20));
            Load(InYear());
            var textWith200 = _counter.Commands.First();

            Assert.AreEqual(textWith10.Length, textWith200.Length, "The flights query must not embed history");
        }

        /// <summary>Forget tracked entities, so later reads go to the database (as a fresh request would).</summary>
        private void Detach()
        {
            foreach (var entry in _db.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }

        private sealed class CommandCounter : IDbCommandInterceptor
        {
            private int _count;
            public ConcurrentQueue<string> Commands { get; private set; } = new ConcurrentQueue<string>();
            public int Count => _count;

            public void Reset()
            {
                _count = 0;
                Commands = new ConcurrentQueue<string>();
            }

            private void Record(DbCommand command)
            {
                System.Threading.Interlocked.Increment(ref _count);
                Commands.Enqueue(command.CommandText);
            }

            public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext) => Record(command);
            public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext) { }
            public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext) => Record(command);
            public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext) { }
            public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext) => Record(command);
            public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext) { }
        }

        /// <summary>Synthetic clubs, pilots, plane and training catalogue; never real member data.</summary>
        private sealed class SyntheticTrainingData
        {
            private static readonly DateTime Created = new DateTime(2000, 1, 1);
            private readonly string _tag = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();

            public Location Home { get; private set; }
            public Location Away { get; private set; }
            public Club HomeClub { get; private set; }
            public Club AwayClub { get; private set; }
            public Pilot Student { get; private set; }
            public Pilot Instructor { get; private set; }
            public Pilot AwayPilot { get; private set; }
            public Plane Plane { get; private set; }
            public StartType StartType { get; private set; }
            public Training2Program Program { get; private set; }
            public Training2Program OtherProgram { get; private set; }
            public Training2Lesson LessonA { get; private set; }
            public Training2Lesson LessonB { get; private set; }
            public Training2Exercise Exercise1 { get; private set; }
            public Training2Exercise Exercise2 { get; private set; }
            public Grading Grading { get; private set; }
            public CommentaryType Phase { get; private set; }
            public Commentary Comment { get; private set; }
            public Manouvre Manoeuvre { get; private set; }

            public static SyntheticTrainingData Create(FlightContext db)
            {
                var d = new SyntheticTrainingData();
                d.Home = db.Locations.Add(d.NewLocation("Home"));
                d.Away = db.Locations.Add(d.NewLocation("Away"));
                db.SaveChanges();
                d.HomeClub = db.Clubs.Add(new Club { ShortName = "H" + d._tag, Name = "Test Home " + d._tag, LocationId = d.Home.LocationId });
                d.AwayClub = db.Clubs.Add(new Club { ShortName = "A" + d._tag, Name = "Test Away " + d._tag, LocationId = d.Away.LocationId });
                db.SaveChanges();
                d.Student = db.Pilots.Add(new Pilot { Name = "Test Student " + d._tag, MemberId = "S" + d._tag, UnionId = "US" + d._tag, ClubId = d.HomeClub.ClubId });
                d.Instructor = db.Pilots.Add(new Pilot { Name = "Test Instructor " + d._tag, MemberId = "I" + d._tag, UnionId = "UI" + d._tag, InstructorId = "FI" + d._tag, ClubId = d.HomeClub.ClubId });
                d.AwayPilot = db.Pilots.Add(new Pilot { Name = "Test Away Pilot " + d._tag, MemberId = "A" + d._tag, UnionId = "UA" + d._tag, ClubId = d.AwayClub.ClubId });
                d.StartType = db.StartTypes.Add(new StartType { ShortName = "T" + d._tag.Substring(0, 3), Name = "Test winch " + d._tag });
                d.Plane = db.Planes.Add(new Plane
                {
                    Registration = "OY-" + d._tag, CompetitionId = d._tag.Substring(0, 3), Type = "Test glider", Seats = 2, Engines = 0,
                    CreatedTimestamp = Created, LastUpdatedTimestamp = Created, CreatedBy = "test", LastUpdatedBy = "test"
                });
                d.Program = db.TrainingPrograms.Add(new Training2Program("P" + d._tag, "Test program " + d._tag, ""));
                d.OtherProgram = db.TrainingPrograms.Add(new Training2Program("Q" + d._tag, "Test other program " + d._tag, ""));
                d.LessonA = db.TrainingLessons.Add(new Training2Lesson("Test lesson A " + d._tag, "purpose") { DisplayOrder = 1 });
                d.LessonB = db.TrainingLessons.Add(new Training2Lesson("Test lesson B " + d._tag, "purpose") { DisplayOrder = 2 });
                d.Exercise1 = db.TrainingExercises.Add(new Training2Exercise("Test exercise 1 " + d._tag) { DisplayOrder = 1 });
                d.Exercise2 = db.TrainingExercises.Add(new Training2Exercise("Test exercise 2 " + d._tag) { DisplayOrder = 2 });
                d.Grading = db.Gradings.Add(new Grading { Name = "Test grade " + d._tag, Value = 3 });
                d.Phase = db.CommentaryTypes.Add(new CommentaryType { CType = "Test phase " + d._tag, DisplayOrder = 1 });
                d.Comment = db.Commentaries.Add(new Commentary { Comment = "Test comment " + d._tag });
                d.Manoeuvre = db.Manouvres.Add(new Manouvre { ManouvreItem = "Test manoeuvre " + d._tag, IconCssClass = "fa-test" });
                db.SaveChanges();
                return d;
            }

            /// <summary>Re-attach the shared entities after the context was cleared, so new flights reference them instead of inserting copies.</summary>
            public void Reattach(FlightContext db)
            {
                Attach(db, Home); Attach(db, Away); Attach(db, HomeClub); Attach(db, AwayClub);
                Attach(db, Student); Attach(db, Instructor); Attach(db, AwayPilot); Attach(db, Plane); Attach(db, StartType);
                Attach(db, Program); Attach(db, OtherProgram); Attach(db, LessonA); Attach(db, LessonB);
                Attach(db, Exercise1); Attach(db, Exercise2); Attach(db, Grading); Attach(db, Phase); Attach(db, Comment); Attach(db, Manoeuvre);
            }

            private static void Attach<T>(FlightContext db, T entity) where T : class
            {
                if (db.Entry(entity).State == EntityState.Detached)
                    db.Set<T>().Attach(entity);
            }

            private Location NewLocation(string name)
            {
                return new Location
                {
                    Name = "Test " + name + " " + _tag, ICAO = "XX" + _tag.Substring(0, 2),
                    CreatedTimestamp = Created, LastUpdatedTimestamp = Created, CreatedBy = "test", LastUpdatedBy = "test"
                };
            }

            /// <summary>Adds dual training flights (student front, instructor back) with exercises, a note, a phase comment and a manoeuvre.</summary>
            public List<Flight> AddTrainingFlights(FlightContext db, int count, DateTime date, Location at = null, Pilot pilot = null, Training2Program program = null, Training2Lesson lesson = null)
            {
                var flights = new List<Flight>();
                for (var i = 0; i < count; i++)
                {
                    var flight = AddFlight(db, date, at ?? Home, pilot ?? Student, Instructor);
                    AddExercise(db, flight, program ?? Program, lesson ?? LessonA, Exercise1);
                    AddExercise(db, flight, program ?? Program, LessonB, Exercise2);
                    var annotation = db.TrainingFlightAnnotations.Add(new TrainingFlightAnnotation { FlightId = flight.FlightId, Note = "Test note" });
                    annotation.Manouvres.Add(Manoeuvre);
                    annotation.TrainingFlightAnnotationCommentCommentTypes.Add(new TrainingFlightAnnotationCommentCommentType
                    {
                        TrainingFlightAnnotation = annotation, Commentary = Comment, CommentaryType = Phase,
                        CommentaryId = Comment.CommentaryId, CommentaryTypeId = Phase.CommentaryTypeId
                    });
                    flights.Add(flight);
                }
                db.SaveChanges();
                return flights;
            }

            public Flight AddFlight(FlightContext db, DateTime date, Location at, Pilot pilot, Pilot backseat)
            {
                var flight = new Flight
                {
                    FlightId = Guid.NewGuid(),
                    Date = date,
                    PlaneId = Plane.PlaneId,
                    PilotId = pilot.PilotId,
                    PilotBackseatId = backseat?.PilotId,
                    BetalerId = pilot.PilotId,
                    StartTypeId = StartType.StartTypeId,
                    StartedFromId = at.LocationId,
                    LandedOnId = at.LocationId,
                    LastUpdatedBy = "test"
                };
                flight.Departure = date.AddHours(10);
                flight.Landing = date.AddHours(10).AddMinutes(25);
                db.Flights.Add(flight);
                return flight;
            }

            public AppliedExercise AddExercise(FlightContext db, Flight flight, Training2Program program, Training2Lesson lesson, Training2Exercise exercise)
            {
                return db.AppliedExercises.Add(new AppliedExercise
                {
                    FlightId = flight.FlightId, Program = program, Lesson = lesson, Exercise = exercise, Grading = Grading, Instructor = Instructor
                });
            }
        }
    }
}
