using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Linq;
using System.Transactions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyntheticReportData = FlightJournal.Tests.Integration.ReportLoadTest.SyntheticReportData;

namespace FlightJournal.Tests.Integration
{
    /// <summary>
    /// The report data stamp hook in FlightContext.SaveChanges and the AvailableDates snapshot (BUG-0730b3). Runs against the
    /// LocalDB development database. Every test works inside a TransactionScope that is never completed, so all synthetic
    /// data is rolled back. The stamp is process-wide, so tests compare deltas and per-date stamps.
    /// </summary>
    [TestClass]
    public class AvailableDatesCacheTest
    {
        // A year with no seeded demo flights, so periods inside it only see this test's synthetic data
        private const int Year = 2001;

        private TransactionScope _scope;
        private FlightContext _db;
        private SqlCommandCounter _counter;

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
            AvailableDatesCache.ResetForTests();
            _scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(5) });
            _db = new FlightContext();
            _counter = new SqlCommandCounter();
            DbInterception.Add(_counter);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DbInterception.Remove(_counter);
            _db.Dispose();
            _scope.Dispose(); // never completed: rolls back
            AvailableDatesCache.ResetForTests();
        }

        /// <summary>Forget tracked entities, so later reads go to the database (as a fresh request would).</summary>
        private void Detach()
        {
            foreach (var entry in _db.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }

        private static long LastChange(DateTime date) => ReportDataStamp.LastChange(ReportPeriod.ForDay(date));

        [TestMethod]
        [TestCategory("Integration")]
        public void SaveChanges_AddedModifiedDeleted_TouchesDates()
        {
            var data = SyntheticReportData.Create(_db);
            var d1 = new DateTime(Year, 2, 1);
            var d2 = new DateTime(Year, 2, 2);
            var d3 = new DateTime(Year, 2, 3);
            var d4 = new DateTime(Year, 2, 4);

            // Added
            var flight = data.AddFlight(_db, d1, data.Home, data.HomePilot, null, data.HomePilot);
            var before = ReportDataStamp.Current;
            _db.SaveChanges();
            Assert.IsTrue(ReportDataStamp.Current > before, "Added");
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d1));

            // Tracked date move: the old and the new date
            flight.Date = d2;
            _db.SaveChanges();
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d1), "Tracked move: old date");
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d2), "Tracked move: new date");

            // Disconnected date move, as FlightController.Edit attaches the posted flight as Modified: old date from the database
            var flightId = flight.FlightId;
            Detach();
            var posted = _db.Flights.AsNoTracking().Single(f => f.FlightId == flightId);
            posted.Date = d3;
            _db.Entry(posted).State = EntityState.Modified;
            _db.SaveChanges();
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d2), "Disconnected move: old date (read from the database)");
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d3), "Disconnected move: new date");

            // Soft delete
            Detach();
            var tracked = _db.Flights.Single(f => f.FlightId == flightId);
            var beforeSoftDelete = ReportDataStamp.Current;
            tracked.Deleted = d3;
            _db.SaveChanges();
            Assert.IsTrue(ReportDataStamp.Current > beforeSoftDelete, "Soft delete");
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d3));

            // Hard delete: the original date
            var other = data.AddFlight(_db, d4, data.Home, data.HomePilot2, null, data.HomePilot2);
            _db.SaveChanges();
            var beforeHardDelete = ReportDataStamp.Current;
            _db.Flights.Remove(other);
            _db.SaveChanges();
            Assert.IsTrue(ReportDataStamp.Current > beforeHardDelete, "Hard delete");
            Assert.AreEqual(ReportDataStamp.Current, LastChange(d4));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SaveChanges_Failure_DoesNotTouch()
        {
            var data = SyntheticReportData.Create(_db);
            var flight = data.AddFlight(_db, new DateTime(Year, 3, 1), data.Home, data.HomePilot, null, data.HomePilot);
            flight.PlaneId = int.MaxValue; // no such plane: the insert fails on the foreign key
            var before = ReportDataStamp.Current;

            try
            {
                _db.SaveChanges();
                Assert.Fail("The save should fail on the foreign key");
            }
            catch (DbUpdateException)
            {
            }

            Assert.AreEqual(before, ReportDataStamp.Current, "Nothing is touched when the save fails");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SaveChanges_PilotClubChange_TouchesAll()
        {
            var data = SyntheticReportData.Create(_db);

            var before = ReportDataStamp.Current;
            data.HomePilot.ClubId = data.AwayClub.ClubId;
            _db.SaveChanges();
            Assert.IsTrue(ReportDataStamp.Current > before, "A pilot moving club changes club counts");

            var beforeLocation = ReportDataStamp.Current;
            data.HomeClub.LocationId = data.Away.LocationId;
            _db.SaveChanges();
            Assert.IsTrue(ReportDataStamp.Current > beforeLocation, "A club moving location changes club counts");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SaveChanges_UnrelatedChange_DoesNotTouch()
        {
            var data = SyntheticReportData.Create(_db);

            var before = ReportDataStamp.Current;
            data.HomePilot.Name = data.HomePilot.Name + " renamed";
            _db.SaveChanges();
            Assert.AreEqual(before, ReportDataStamp.Current, "A tracked pilot rename leaves ClubId unmodified");

            data.Plane.Note = "Test note";
            _db.SaveChanges();
            Assert.AreEqual(before, ReportDataStamp.Current, "A plane edit doesn't change report data");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void AvailableDates_Warm_ZeroCommands_OneRebuildAfterChange()
        {
            var data = SyntheticReportData.Create(_db);
            var d1 = new DateTime(Year, 4, 1);
            var d2 = new DateTime(Year, 4, 2);
            data.AddFlight(_db, d1, data.Home, data.HomePilot, null, data.HomePilot);
            data.AddFlight(_db, d2, data.Home, data.HomePilot, null, data.HomePilot);
            data.AddFlight(_db, d2, data.Home, data.HomePilot2, null, data.HomePilot2);
            _db.SaveChanges();
            Detach();

            _counter.Reset();
            var cold = AvailableDatesCache.Get(_db, data.HomeFilter);
            Assert.AreEqual(1, _counter.Count, "Cold: one query");
            Assert.AreEqual(1, cold[d1]);
            Assert.AreEqual(2, cold[d2]);

            _counter.Reset();
            var warm = AvailableDatesCache.Get(_db, data.HomeFilter);
            Assert.AreEqual(0, _counter.Count, "Warm: no query");
            Assert.AreSame(cold, warm);

            data.AddFlight(_db, d1, data.Home, data.HomePilot3, null, data.HomePilot3);
            _db.SaveChanges();
            _counter.Reset();
            var rebuilt = AvailableDatesCache.Get(_db, data.HomeFilter);
            Assert.AreEqual(1, _counter.Count, "After a change: one rebuild query");
            Assert.AreEqual(2, rebuilt[d1]);

            _counter.Reset();
            var warmAgain = AvailableDatesCache.Get(_db, data.HomeFilter);
            Assert.AreEqual(0, _counter.Count, "Warm again");
            Assert.AreSame(rebuilt, warmAgain);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void AvailableDates_ClubFilter_MatchesLegacy_ExcludesDeleted()
        {
            var data = SyntheticReportData.Create(_db);
            var homePilotAway = data.AddFlight(_db, new DateTime(Year, 5, 1), data.Away, data.HomePilot, null, data.HomePilot);
            var awayAtHome = data.AddFlight(_db, new DateTime(Year, 5, 2), data.Home, data.AwayPilot, null, data.AwayPilot);
            var awayLandsAtHome = data.AddFlight(_db, new DateTime(Year, 5, 3), data.Away, data.AwayPilot, null, data.AwayPilot);
            awayLandsAtHome.LandedOnId = data.Home.LocationId;
            var homePaysAway = data.AddFlight(_db, new DateTime(Year, 5, 4), data.Away, data.AwayPilot, null, data.HomePilot);
            var otherClubOnly = data.AddFlight(_db, new DateTime(Year, 5, 5), data.Away, data.AwayPilot, data.AwayPilot2, data.AwayPilot);
            var deletedHome = data.AddFlight(_db, new DateTime(Year, 5, 6), data.Home, data.HomePilot, null, data.HomePilot);
            deletedHome.Deleted = deletedHome.Date;
            _db.SaveChanges();
            Detach();

            // The legacy AvailableDates query, with the current club's values captured
            var clubId = data.HomeClub.ClubId;
            var locationId = data.Home.LocationId;
            var legacy = _db.Flights.Where(f => f.Deleted == null)
                .Where(f => f.StartedFromId == locationId
                            || f.LandedOnId == locationId
                            || (f.Pilot != null && f.Pilot.ClubId == clubId)
                            || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                            || (f.Betaler != null && f.Betaler.ClubId == clubId))
                .GroupBy(p => p.Date)
                .Select(g => new { Date = g.Key, FlightCount = g.Count() })
                .ToDictionary(x => x.Date, x => x.FlightCount);

            var dates = AvailableDatesCache.Get(_db, data.HomeFilter);

            CollectionAssert.AreEquivalent(legacy.ToList(), dates.ToList());
            CollectionAssert.AreEquivalent(new[] { homePilotAway.Date, awayAtHome.Date, awayLandsAtHome.Date, homePaysAway.Date }, dates.Keys.ToList());
            Assert.IsFalse(dates.ContainsKey(otherClubOnly.Date), "Another club's flight away is not included");
            Assert.IsFalse(dates.ContainsKey(deletedHome.Date), "Deleted flights are not counted");
        }
    }
}
