using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Transactions;
using FlightJournal.Web.Migrations.FlightContext;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightContext = FlightJournal.Web.Models.FlightContext;

namespace FlightJournal.Tests.Integration
{
    /// <summary>
    /// FEAT-004 demo training seed. Runs against the LocalDB development database. Every test works inside a
    /// TransactionScope that is never completed, so all synthetic data is rolled back.
    /// </summary>
    [TestClass]
    public class DemoTrainingSeederTest
    {
        private TransactionScope _scope;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Same database as the RouteFilters tests
            var folder = Directory.GetDirectoryRoot(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\FlightJournal.Web\\App_Data"));
            AppDomain.CurrentDomain.SetData("DataDirectory", folder);
            // The counting context shares FlightContext's database; it must not run its own initializer
            Database.SetInitializer<CountingFlightContext>(null);
            // Run migrations and seeding outside any test transaction
            using (var db = new FlightContext())
                db.Database.Initialize(false);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(5) });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _scope.Dispose(); // never completed: rolls back
        }

        // A fresh GUID sequence per run, so seeded flight ids never collide with rows left in the shared test database
        private static int GuidSeed => Guid.NewGuid().GetHashCode();

        [TestMethod]
        [TestCategory("Integration")]
        public void Apply_WithoutPrograms_CreatesNoTrainingFlights_NoException()
        {
            using (var db = new FlightContext())
            {
                if (db.TrainingPrograms.Any())
                    Assert.Inconclusive("The test database already has training programmes");
                var data = SyntheticActors.Create(db);
                var flightsBefore = db.Flights.Count();

                var created = data.Apply(db, DemoTrainingScenario.Plan(DateTime.Today, days: 30));

                Assert.AreEqual(0, created);
                Assert.AreEqual(flightsBefore, db.Flights.Count());
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Apply_WithMinimalProgram_CreatesFlightsExercisesAnnotations_AsPlanned()
        {
            using (var db = new FlightContext())
            {
                if (db.TrainingPrograms.Any())
                    Assert.Inconclusive("The test database already has training programmes");
                AddMinimalProgram(db);
                var data = SyntheticActors.Create(db);
                var plan = DemoTrainingScenario.Plan(DateTime.Today, days: 30);

                var created = data.Apply(db, plan);

                var pilotIds = data.Actors.Values.Select(p => p.PilotId).ToList();
                var flights = db.Flights.Where(f => pilotIds.Contains(f.PilotId)).ToList();
                var flightIds = flights.Select(f => f.FlightId).ToList();
                var exercises = db.AppliedExercises.Where(a => flightIds.Contains(a.FlightId)).ToList();
                var annotations = db.TrainingFlightAnnotations.Include(a => a.TrainingFlightAnnotationCommentCommentTypes).Include(a => a.Manouvres)
                    .Where(a => flightIds.Contains(a.FlightId)).ToList();

                Assert.AreEqual(plan.Count, created);
                Assert.AreEqual(plan.Count, flights.Count);
                Assert.AreEqual(plan.Sum(p => p.ExerciseCount), exercises.Count);
                Assert.AreEqual(plan.Count(p => p.HasAnnotation), annotations.Count);
                Assert.AreEqual(plan.Count(p => p.HasNote), annotations.Count(a => !string.IsNullOrEmpty(a.Note)));
                Assert.AreEqual(plan.Count(p => p.HasPhaseComments), annotations.Count(a => a.TrainingFlightAnnotationCommentCommentTypes.Any()));
                Assert.IsTrue(annotations.All(a => a.Manouvres.Count >= 1));
                Assert.IsTrue(flights.All(f => f.HasTrainingData));
                Assert.AreEqual(1, flights.Count(f => f.Deleted != null));
                Assert.AreEqual(DateTime.Today.AddDays(-10), flights.Single(f => f.Deleted != null).Date);
                Assert.AreEqual(plan.Count(p => p.Seats == SeatPattern.StudentSolo), flights.Count(f => f.PilotBackseatId == null));
                Assert.IsTrue(flights.Where(f => f.PilotBackseatId == null).All(f => f.PlaneId == data.SingleSeater.PlaneId));
                Assert.IsTrue(exercises.All(a => a.Instructor != null && a.Grading != null));
                Assert.IsTrue(flights.Where(f => f.Departure == f.Landing).All(f => plan.Any(p => p.IsBriefingOnly && p.Date == f.Date)));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Apply_SaveChangesIsBatched()
        {
            using (var db = new CountingFlightContext())
            {
                if (db.TrainingPrograms.Any())
                    Assert.Inconclusive("The test database already has training programmes");
                AddMinimalProgram(db);
                var data = SyntheticActors.Create(db);
                var plan = DemoTrainingScenario.Plan(DateTime.Today, days: 200);
                Assert.IsTrue(plan.Count > DemoTrainingSeeder.BatchSize, "The plan must span several batches");
                db.SaveChangesCalls = 0;

                var created = data.Apply(db, plan);

                var maxBatches = (int)Math.Ceiling(created / (double)DemoTrainingSeeder.BatchSize) + 1;
                Console.WriteLine($"{created} flights, {db.SaveChangesCalls} SaveChanges calls (max {maxBatches})");
                Assert.AreEqual(plan.Count, created);
                Assert.IsTrue(db.SaveChangesCalls <= maxBatches, db.SaveChangesCalls.ToString());
                Assert.IsTrue(db.Configuration.AutoDetectChangesEnabled, "Change detection is restored afterwards");
            }
        }

        /// <summary>One synthetic programme with a practical and a briefing-only exercise; gradings, commentaries and manoeuvres come from the seed.</summary>
        private static void AddMinimalProgram(FlightContext db)
        {
            var tag = Guid.NewGuid().ToString("N").Substring(0, 6);
            var practical = new Training2Exercise("Test practical " + tag) { DisplayOrder = 1 };
            var briefing = new Training2Exercise("Test briefing " + tag, briefingOnly: true) { DisplayOrder = 2 };
            var lesson = new Training2Lesson("Test lesson " + tag, "purpose") { DisplayOrder = 1, Exercises = new List<Training2Exercise> { practical, briefing } };
            db.TrainingPrograms.Add(new Training2Program("SPL-S", "Test programme " + tag, "") { Lessons = new List<Training2Lesson> { lesson } });
            db.SaveChanges();
            Assert.IsTrue(db.Gradings.Any() && db.Commentaries.Any() && db.Manouvres.Any(), "The test seed provides gradings, commentaries and manoeuvres");
        }

        /// <summary>Counts SaveChanges calls (each one is a round of inserts and its own audit rows).</summary>
        private sealed class CountingFlightContext : FlightContext
        {
            public int SaveChangesCalls;

            public override int SaveChanges()
            {
                SaveChangesCalls++;
                return base.SaveChanges();
            }
        }

        /// <summary>Synthetic clubs, actors, locations and planes; never real member data.</summary>
        private sealed class SyntheticActors
        {
            private readonly string _tag = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
            public Dictionary<string, Pilot> Actors { get; } = new Dictionary<string, Pilot>();
            public Dictionary<string, Location> Locations { get; } = new Dictionary<string, Location>();
            public Plane TwoSeater { get; private set; }
            public Plane SingleSeater { get; private set; }
            public StartType Start { get; private set; }

            public static SyntheticActors Create(FlightContext db)
            {
                var d = new SyntheticActors();
                var main = db.Locations.Add(new Location { Name = "Test main " + d._tag });
                var other = db.Locations.Add(new Location { Name = "Test other " + d._tag });
                db.SaveChanges();
                var mainClub = db.Clubs.Add(new Club { ShortName = "M" + d._tag, Name = "Test main club " + d._tag, LocationId = main.LocationId });
                var otherClub = db.Clubs.Add(new Club { ShortName = "O" + d._tag, Name = "Test other club " + d._tag, LocationId = other.LocationId });
                db.SaveChanges();
                d.Locations[TrainingScenarioActors.MainLocation] = main;
                d.Locations[TrainingScenarioActors.OtherLocation] = other;
                d.Actors[TrainingScenarioActors.StudentA] = db.Pilots.Add(d.NewPilot("SA", mainClub, null));
                d.Actors[TrainingScenarioActors.StudentB] = db.Pilots.Add(d.NewPilot("SB", mainClub, null));
                d.Actors[TrainingScenarioActors.StudentOther] = db.Pilots.Add(d.NewPilot("SC", otherClub, null));
                d.Actors[TrainingScenarioActors.InstructorA] = db.Pilots.Add(d.NewPilot("IA", mainClub, "FIA" + d._tag));
                d.Actors[TrainingScenarioActors.InstructorOther] = db.Pilots.Add(d.NewPilot("IB", otherClub, "FIB" + d._tag));
                d.Start = db.StartTypes.Add(new StartType { ShortName = "T", Name = "Test winch " + d._tag });
                d.TwoSeater = db.Planes.Add(new Plane { Registration = "OY-2" + d._tag, CompetitionId = "T2", Type = "Test two-seater", Seats = 2 });
                d.SingleSeater = db.Planes.Add(new Plane { Registration = "OY-1" + d._tag, CompetitionId = "T1", Type = "Test single-seater", Seats = 1 });
                db.SaveChanges();
                return d;
            }

            public int Apply(FlightContext db, IReadOnlyList<PlannedTrainingFlight> plan)
            {
                return DemoTrainingSeeder.Apply(db, plan, Actors, Locations, TwoSeater, SingleSeater, Start, GuidSeed);
            }

            private Pilot NewPilot(string key, Club club, string instructorId)
            {
                return new Pilot { Name = "Test " + key + " " + _tag, MemberId = key + _tag, ClubId = club.ClubId, InstructorId = instructorId };
            }
        }
    }
}
