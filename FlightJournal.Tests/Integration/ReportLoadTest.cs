using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Linq;
using System.Transactions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Integration
{
    /// <summary>
    /// Report/Index data loading (PERF-001 step 1). Runs against the LocalDB development database. Every test works
    /// inside a TransactionScope that is never completed, so all synthetic data is rolled back.
    /// </summary>
    [TestClass]
    public class ReportLoadTest
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
        }

        /// <summary>
        /// Reproduces the pre-PERF-001 pattern: the QueryableFlights setter (Flights, ClubFlights, DistinctLocations),
        /// the Clubs GROUP BY, the Month view's distinct pilot counts per day and the co-pilot MaskedName lazy loads.
        /// </summary>
        private int CountOldMonthPatternCommands(SyntheticReportData data, int month)
        {
            Detach();
            _counter.Reset();
            var clubId = data.HomeClub.ClubId;
            var locationId = data.Home.LocationId;
            var flights = _db.Flights.Where(f => f.Date.Month == month && f.Date.Year == Year && f.Deleted == null)
                .Include("Plane").Include("StartedFrom").Include("LandedOn").Include("Pilot").Include("Pilot.Club").Include("PilotBackseat").Include("Betaler").Include("StartType")
                .Where(f => f.StartedFromId == locationId || f.LandedOnId == locationId
                            || (f.Pilot != null && f.Pilot.ClubId == clubId)
                            || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                            || (f.Betaler != null && f.Betaler.ClubId == clubId))
                .OrderBy(o => o.Departure)
                .AsNoTracking();
            var list = flights.ToList();
            var clubFlightsQuery = LegacyClubFlights(flights, clubId);
            var clubFlights = clubFlightsQuery.ToList();
            var locations = clubFlightsQuery.Select(d => d.StartedFrom).Distinct().OrderBy(d => d.Name).ToList();
            var clubs = clubFlightsQuery.Where(f => f.Deleted == null).GroupBy(f => f.Pilot.Club).OrderByDescending(o => o.Count()).ToList();
            var pilots = LegacyDistinctPilotCount(clubFlightsQuery);
            foreach (var date in list.Select(f => f.Date).Distinct())
                pilots += LegacyDistinctPilotCount(clubFlightsQuery.Where(s => s.Date == date));
            var names = clubFlights.Select(f => f.Pilot.MaskedName(false) + f.PilotBackseat.MaskedName(false)).ToList();
            Assert.IsTrue(pilots > 0 && names.Count > 0 && locations.Count > 0 && clubs.Count > 0);
            return _counter.Count;
        }

        private static IQueryable<Flight> LegacyClubFlights(IQueryable<Flight> flights, int clubId)
        {
            return flights.Where(f => (f.Pilot != null && f.Pilot.ClubId == clubId)
                                      || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                                      || (f.Betaler != null && f.Betaler.ClubId == clubId)
                                      || (f.Pilot == null && f.PilotBackseat == null && f.Betaler == null));
        }

        private static int LegacyDistinctPilotCount(IQueryable<Flight> clubFlights)
        {
            return clubFlights.Select(s => s.Pilot).Union(clubFlights.Where(s => s.PilotBackseat != null).Select(s => s.PilotBackseat).Distinct()).Distinct().Count();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void OldPattern_QueryCountGrowsWithMonthDays()
        {
            var data = SyntheticReportData.Create(_db);
            data.AddFlightsOverDays(_db, Year, 3, 3);
            var countAt3 = CountOldMonthPatternCommands(data, 3);

            data.AddFlightsOverDays(_db, Year, 3, 28);
            var countAt28 = CountOldMonthPatternCommands(data, 3);

            Detach();
            _counter.Reset();
            _db.Flights.Where(f => f.Date.Year == Year && f.Deleted == null).AsNoTracking().ToList();
            var yearSql = _counter.Commands.Single();

            Console.WriteLine($"Old month pattern SQL commands: 3 days -> {countAt3}, 28 days -> {countAt28}");
            Assert.IsTrue(countAt28 > countAt3, "The old pattern's command count grows with the number of flying days");
            StringAssert.Contains(yearSql, "DATEPART", "The old year predicate is not index friendly");
        }

        /// <summary>The new pattern: one load, then everything the Year/Month/Index views read, in memory.</summary>
        private ReportViewModel LoadAndRender(ReportPeriod period, ReportClubFilter club, bool excludeDeleted = true)
        {
            Detach();
            _counter.Reset();
            var rpt = new ReportViewModel();
            rpt.Load(ReportDataLoader.LoadFlights(_db, period, club, excludeDeleted), club);
            TouchEverythingTheViewsRead(rpt, period);
            return rpt;
        }

        private static void TouchEverythingTheViewsRead(ReportViewModel rpt, ReportPeriod period)
        {
            var touched = new List<object>();
            foreach (var club in rpt.Clubs)
                touched.Add(club.Key.ShortName + club.Key.Name + club.Sum(f => f.LandingCount) + club.Sum(f => f.Duration.Ticks));
            foreach (var location in rpt.DistinctLocations)
                touched.Add(location.Name + location.LocationId);
            touched.Add(rpt.DistinctClubPilotCount());
            for (var month = 1; month <= 12; month++)
                touched.Add(rpt.DistinctClubPilotCount(f => f.Date.Month == month));
            foreach (var date in rpt.Flights.Select(f => f.Date).Distinct())
                touched.Add(rpt.DistinctClubPilotCount(f => f.Date == date));
            foreach (var f in rpt.Flights)
            {
                touched.Add(f.Pilot.MaskedName(false) + f.PilotBackseat.MaskedName(false) + f.Pilot.MaskedName(true) + f.PilotBackseat.MaskedName(true));
                touched.Add(f.Pilot.Club.ShortName + f.PilotBackseat?.Club.ShortName + f.Pilot.Name + f.PilotBackseat?.Name);
                touched.Add(f.Plane.RenderName + f.StartType.ShortName + f.StartType.StartTypeId + f.StartedFrom.Name + f.LandedOn?.Name + f.Betaler.Name);
            }
            Assert.IsTrue(touched.Count > 0, period.Kind.ToString());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Year_LoadIsOneCommand_ConstantInFlights()
        {
            var data = SyntheticReportData.Create(_db);
            var club = data.HomeFilter;
            data.AddFlightsOverYear(_db, Year, 10);
            var at10 = LoadAndRender(ReportPeriod.ForYear(Year), club);
            var countAt10 = _counter.Count;

            data.AddFlightsOverYear(_db, Year, 110);
            var at120 = LoadAndRender(ReportPeriod.ForYear(Year), club);
            var countAt120 = _counter.Count;

            Console.WriteLine($"Year SQL commands: N=10 -> {countAt10}, N=120 -> {countAt120}");
            Assert.AreEqual(10, at10.Flights.Count);
            Assert.AreEqual(120, at120.Flights.Count);
            Assert.AreEqual(12, at120.Flights.Select(f => f.Date.Month).Distinct().Count());
            Assert.AreEqual(1, countAt10, string.Join("\n---\n", _counter.Commands));
            Assert.AreEqual(1, countAt120, string.Join("\n---\n", _counter.Commands));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Month_LoadIsOneCommand_ConstantInDays()
        {
            var data = SyntheticReportData.Create(_db);
            var club = data.HomeFilter;
            data.AddFlightsOverDays(_db, Year, 3, 3);
            var at3 = LoadAndRender(ReportPeriod.ForMonth(Year, 3), club);
            var countAt3 = _counter.Count;

            data.AddFlightsOverDays(_db, Year, 3, 28);
            var at28 = LoadAndRender(ReportPeriod.ForMonth(Year, 3), club);
            var countAt28 = _counter.Count;

            Console.WriteLine($"Month SQL commands: 3 days -> {countAt3}, 28 days -> {countAt28}");
            Assert.AreEqual(3, at3.Flights.Select(f => f.Date).Distinct().Count());
            Assert.AreEqual(28, at28.Flights.Select(f => f.Date).Distinct().Count());
            Assert.AreEqual(1, countAt3, string.Join("\n---\n", _counter.Commands));
            Assert.AreEqual(1, countAt28, string.Join("\n---\n", _counter.Commands));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void NoLazyLoads_AfterLoad()
        {
            var data = SyntheticReportData.Create(_db);
            // Masked clubs on both seats, so MaskedName(false) has to read Pilot.Club and PilotBackseat.Club
            data.HomeClub.MaskPilotIdentityIfUserIsNotLoggedIn = true;
            data.AwayClub.MaskPilotIdentityIfUserIsNotLoggedIn = true;
            _db.SaveChanges();
            data.AddFlight(_db, new DateTime(Year, 5, 1), data.Home, data.HomePilot, data.AwayPilot, data.HomePilot);
            data.AddFlight(_db, new DateTime(Year, 5, 1), data.Away, data.AwayPilot, data.HomePilot2, data.AwayPilot);
            _db.SaveChanges();

            var rpt = LoadAndRender(ReportPeriod.ForDay(new DateTime(Year, 5, 1)), data.HomeFilter, excludeDeleted: false);

            Assert.AreEqual(2, rpt.Flights.Count);
            Assert.IsTrue(rpt.Flights.All(f => f.PilotBackseat.MaskedName(false).StartsWith("N.N")));
            Assert.AreEqual(1, _counter.Count, "Only the load itself; no lazy loads:\n" + string.Join("\n---\n", _counter.Commands));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void YearAndMonth_UseDateRange_NotDatepart()
        {
            var data = SyntheticReportData.Create(_db);
            data.AddFlightsOverDays(_db, Year, 3, 2);

            foreach (var period in new[] { ReportPeriod.ForYear(Year), ReportPeriod.ForMonth(Year, 3), ReportPeriod.ForDay(new DateTime(Year, 3, 1)) })
            {
                LoadAndRender(period, data.HomeFilter);
                var sql = _counter.Commands.Single();
                StringAssert.Contains(sql, "[Date] >= ", period.Kind.ToString());
                StringAssert.Contains(sql, "[Date] < ", period.Kind.ToString());
                Assert.IsFalse(sql.Contains("DATEPART"), period.Kind + " must not use DATEPART:\n" + sql);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ClubFlights_MatchesLegacyPredicate()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 6, 1);
            var homePilotAway = data.AddFlight(_db, date, data.Away, data.HomePilot, null, data.HomePilot);
            var homeBackseatAway = data.AddFlight(_db, date, data.Away, data.AwayPilot, data.HomePilot, data.AwayPilot);
            var homePaysAway = data.AddFlight(_db, date, data.Away, data.AwayPilot, null, data.HomePilot);
            var awayAtHome = data.AddFlight(_db, date, data.Home, data.AwayPilot, data.AwayPilot2, data.AwayPilot);
            var awayLandsAtHome = data.AddFlight(_db, date, data.Away, data.AwayPilot, null, data.AwayPilot);
            awayLandsAtHome.LandedOnId = data.Home.LocationId;
            var otherClubOnly = data.AddFlight(_db, date, data.Away, data.AwayPilot, data.AwayPilot2, data.AwayPilot);
            _db.SaveChanges();
            Detach();

            // Legacy, in SQL: the 6-way base filter, then the 4-way club filter on top
            var clubId = data.HomeClub.ClubId;
            var locationId = data.Home.LocationId;
            var legacyBase = _db.Flights.Where(f => f.Date == date)
                .Where(f => f.StartedFromId == locationId || f.LandedOnId == locationId
                            || (f.Pilot != null && f.Pilot.ClubId == clubId)
                            || (f.PilotBackseat != null && f.PilotBackseat.ClubId == clubId)
                            || (f.Betaler != null && f.Betaler.ClubId == clubId))
                .AsNoTracking();
            var legacyFlights = legacyBase.Select(f => f.FlightId).ToList();
            var legacyClubFlights = LegacyClubFlights(legacyBase, clubId).Select(f => f.FlightId).ToList();

            var rpt = LoadAndRender(ReportPeriod.ForDay(date), data.HomeFilter, excludeDeleted: false);

            CollectionAssert.AreEquivalent(new[] { homePilotAway.FlightId, homeBackseatAway.FlightId, homePaysAway.FlightId, awayAtHome.FlightId, awayLandsAtHome.FlightId }, rpt.Flights.Select(f => f.FlightId).ToList());
            CollectionAssert.AreEquivalent(new[] { homePilotAway.FlightId, homeBackseatAway.FlightId, homePaysAway.FlightId }, rpt.ClubFlights.Select(f => f.FlightId).ToList());
            CollectionAssert.AreEquivalent(legacyFlights, rpt.Flights.Select(f => f.FlightId).ToList());
            CollectionAssert.AreEquivalent(legacyClubFlights, rpt.ClubFlights.Select(f => f.FlightId).ToList());
            Assert.IsFalse(rpt.Flights.Any(f => f.FlightId == otherClubOnly.FlightId));

            // No club: ClubFlights is every flight of the period
            var unfiltered = LoadAndRender(ReportPeriod.ForDay(date), ReportClubFilter.Unfiltered, excludeDeleted: false);
            var ours = new[] { homePilotAway, homeBackseatAway, homePaysAway, awayAtHome, awayLandsAtHome, otherClubOnly }.Select(f => f.FlightId).ToList();
            CollectionAssert.IsSubsetOf(ours, unfiltered.Flights.Select(f => f.FlightId).ToList());
            CollectionAssert.AreEquivalent(unfiltered.Flights.Select(f => f.FlightId).ToList(), unfiltered.ClubFlights.Select(f => f.FlightId).ToList());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DistinctClubPilotCount_CountsEachPersonOnce_AcrossBothSeats()
        {
            var data = SyntheticReportData.Create(_db);
            var day1 = new DateTime(Year, 7, 1);
            var day2 = new DateTime(Year, 7, 2);
            data.AddFlight(_db, day1, data.Home, data.HomePilot, data.HomePilot2, data.HomePilot);   // A front, B back
            data.AddFlight(_db, day1, data.Home, data.HomePilot2, null, data.HomePilot2);            // B front
            data.AddFlight(_db, day2, data.Home, data.HomePilot3, data.HomePilot, data.HomePilot3);  // C front, A back
            data.AddFlight(_db, day2, data.Home, data.HomePilot, null, data.HomePilot);              // A front again
            _db.SaveChanges();
            Detach();
            var clubId = data.HomeClub.ClubId;
            var legacyMonth = LegacyClubFlights(_db.Flights.Where(f => f.Date.Month == 7 && f.Date.Year == Year && f.Deleted == null), clubId);
            var legacyTotal = LegacyDistinctPilotCount(legacyMonth);
            var legacyDay1 = LegacyDistinctPilotCount(legacyMonth.Where(s => s.Date == day1));
            var legacyDay2 = LegacyDistinctPilotCount(legacyMonth.Where(s => s.Date == day2));

            var rpt = LoadAndRender(ReportPeriod.ForMonth(Year, 7), data.HomeFilter);

            Assert.AreEqual(3, rpt.DistinctClubPilotCount());
            Assert.AreEqual(2, rpt.DistinctClubPilotCount(f => f.Date == day1));
            Assert.AreEqual(2, rpt.DistinctClubPilotCount(f => f.Date == day2));
            Assert.AreEqual(0, rpt.DistinctClubPilotCount(f => f.Date.Month == 8));
            Assert.AreEqual(legacyTotal, rpt.DistinctClubPilotCount());
            Assert.AreEqual(legacyDay1, rpt.DistinctClubPilotCount(f => f.Date == day1));
            Assert.AreEqual(legacyDay2, rpt.DistinctClubPilotCount(f => f.Date == day2));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Clubs_GroupedByClubId_OrderedByCount_KeyIsClub()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 8, 1);
            data.AddFlight(_db, date, data.Away, data.AwayPilot, null, data.AwayPilot);
            data.AddFlight(_db, date, data.Home, data.HomePilot, null, data.HomePilot);
            data.AddFlight(_db, date, data.Home, data.HomePilot2, null, data.HomePilot2);
            data.AddFlight(_db, date, data.Home, data.HomePilot3, null, data.HomePilot3);
            var deleted = data.AddFlight(_db, date, data.Away, data.AwayPilot2, null, data.AwayPilot2);
            deleted.Deleted = date;
            _db.SaveChanges();

            var rpt = LoadAndRender(ReportPeriod.ForDay(date), ReportClubFilter.Unfiltered, excludeDeleted: false);
            var ours = rpt.Clubs.Where(g => g.Key.ClubId == data.HomeClub.ClubId || g.Key.ClubId == data.AwayClub.ClubId).ToList();

            Assert.AreEqual(2, ours.Count, "One group per club, even though AsNoTracking gives each flight its own Club instance");
            Assert.AreEqual(data.HomeClub.ShortName, ours[0].Key.ShortName);
            Assert.AreEqual(3, ours[0].Count());
            Assert.AreEqual(data.AwayClub.ShortName, ours[1].Key.ShortName);
            Assert.AreEqual(1, ours[1].Count(), "Deleted flights are not counted");
            var counts = rpt.Clubs.Select(g => g.Count()).ToList();
            CollectionAssert.AreEqual(counts.OrderByDescending(c => c).ToList(), counts, "Ordered by flight count, descending");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DistinctLocations_UniqueOrderedByName()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 9, 1);
            data.AddFlight(_db, date, data.Home, data.HomePilot, null, data.HomePilot);
            data.AddFlight(_db, date, data.Away, data.HomePilot, null, data.HomePilot);
            data.AddFlight(_db, date, data.Home, data.HomePilot2, null, data.HomePilot2);
            data.AddFlight(_db, date, data.Away, data.HomePilot2, null, data.HomePilot2);
            data.AddFlight(_db, date, data.Home, data.AwayPilot, null, data.AwayPilot); // not a club flight: its location only counts via other flights
            _db.SaveChanges();

            var rpt = LoadAndRender(ReportPeriod.ForDay(date), data.HomeFilter, excludeDeleted: false);

            // "Test Away ..." sorts before "Test Home ..."
            CollectionAssert.AreEqual(new[] { data.Away.LocationId, data.Home.LocationId }, rpt.DistinctLocations.Select(l => l.LocationId).ToList());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Day_IncludesDeletedFlights_MonthYearExclude()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 10, 1);
            var kept = data.AddFlight(_db, date, data.Home, data.HomePilot, null, data.HomePilot);
            var deleted = data.AddFlight(_db, date, data.Home, data.HomePilot2, null, data.HomePilot2);
            deleted.Deleted = date;
            _db.SaveChanges();

            var day = LoadAndRender(ReportPeriod.ForDay(date), data.HomeFilter, excludeDeleted: false);
            var month = LoadAndRender(ReportPeriod.ForMonth(Year, 10), data.HomeFilter);
            var year = LoadAndRender(ReportPeriod.ForYear(Year), data.HomeFilter);

            CollectionAssert.AreEquivalent(new[] { kept.FlightId, deleted.FlightId }, day.Flights.Select(f => f.FlightId).ToList());
            CollectionAssert.AreEqual(new[] { kept.FlightId }, month.Flights.Select(f => f.FlightId).ToList());
            CollectionAssert.AreEqual(new[] { kept.FlightId }, year.Flights.Select(f => f.FlightId).ToList());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Load_PeriodBoundaries_StartInclusive_EndExclusive()
        {
            var data = SyntheticReportData.Create(_db);
            var before = data.AddFlight(_db, new DateTime(Year, 2, 28), data.Home, data.HomePilot, null, data.HomePilot);
            var first = data.AddFlight(_db, new DateTime(Year, 3, 1), data.Home, data.HomePilot, null, data.HomePilot);
            var last = data.AddFlight(_db, new DateTime(Year, 3, 31), data.Home, data.HomePilot, null, data.HomePilot);
            var after = data.AddFlight(_db, new DateTime(Year, 4, 1), data.Home, data.HomePilot, null, data.HomePilot);
            _db.SaveChanges();

            var month = LoadAndRender(ReportPeriod.ForMonth(Year, 3), data.HomeFilter);

            CollectionAssert.AreEquivalent(new[] { first.FlightId, last.FlightId }, month.Flights.Select(f => f.FlightId).ToList());
            Assert.IsFalse(month.Flights.Any(f => f.FlightId == before.FlightId || f.FlightId == after.FlightId));
        }

        /// <summary>Forget tracked entities, so later reads go to the database (as a fresh request would).</summary>
        private void Detach()
        {
            foreach (var entry in _db.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }

        /// <summary>Synthetic clubs, pilots, locations and plane; never real member data.</summary>
        private sealed class SyntheticReportData
        {
            private static readonly DateTime Created = new DateTime(2000, 1, 1);
            private readonly string _tag = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
            private int _added;

            public Location Home { get; private set; }
            public Location Away { get; private set; }
            public Club HomeClub { get; private set; }
            public Club AwayClub { get; private set; }
            public Pilot HomePilot { get; private set; }
            public Pilot HomePilot2 { get; private set; }
            public Pilot HomePilot3 { get; private set; }
            public Pilot AwayPilot { get; private set; }
            public Pilot AwayPilot2 { get; private set; }
            public Plane Plane { get; private set; }
            public StartType StartType { get; private set; }

            public ReportClubFilter HomeFilter => ReportClubFilter.From(HomeClub);

            public static SyntheticReportData Create(FlightContext db)
            {
                var d = new SyntheticReportData();
                d.Home = db.Locations.Add(d.NewLocation("Home"));
                d.Away = db.Locations.Add(d.NewLocation("Away"));
                db.SaveChanges();
                d.HomeClub = db.Clubs.Add(new Club { ShortName = "H" + d._tag, Name = "Test Home " + d._tag, LocationId = d.Home.LocationId });
                d.AwayClub = db.Clubs.Add(new Club { ShortName = "A" + d._tag, Name = "Test Away " + d._tag, LocationId = d.Away.LocationId });
                db.SaveChanges();
                d.HomePilot = db.Pilots.Add(d.NewPilot("Home A", d.HomeClub));
                d.HomePilot2 = db.Pilots.Add(d.NewPilot("Home B", d.HomeClub));
                d.HomePilot3 = db.Pilots.Add(d.NewPilot("Home C", d.HomeClub));
                d.AwayPilot = db.Pilots.Add(d.NewPilot("Away A", d.AwayClub));
                d.AwayPilot2 = db.Pilots.Add(d.NewPilot("Away B", d.AwayClub));
                d.StartType = db.StartTypes.Add(new StartType { ShortName = "T" + d._tag.Substring(0, 3), Name = "Test winch " + d._tag });
                d.Plane = db.Planes.Add(new Plane
                {
                    Registration = "OY-" + d._tag, CompetitionId = d._tag.Substring(0, 3), Type = "Test glider", Seats = 2, Engines = 0,
                    CreatedTimestamp = Created, LastUpdatedTimestamp = Created, CreatedBy = "test", LastUpdatedBy = "test"
                });
                db.SaveChanges();
                return d;
            }

            private Pilot NewPilot(string name, Club club)
            {
                var id = name.Replace(" ", "") + _tag;
                return new Pilot { Name = "Test " + name + " " + _tag, MemberId = "M" + id, UnionId = "U" + id, ClubId = club.ClubId };
            }

            private Location NewLocation(string name)
            {
                return new Location
                {
                    Name = "Test " + name + " " + _tag, ICAO = "XX" + _tag.Substring(0, 2),
                    CreatedTimestamp = Created, LastUpdatedTimestamp = Created, CreatedBy = "test", LastUpdatedBy = "test"
                };
            }

            /// <summary>Home club dual flights spread over the 12 months of the year (FK ids only, so no re-attach is needed).</summary>
            public void AddFlightsOverYear(FlightContext db, int year, int count)
            {
                for (var i = 0; i < count; i++, _added++)
                    AddFlight(db, new DateTime(year, _added % 12 + 1, _added / 12 % 28 + 1), Home, HomePilot, HomePilot2, HomePilot);
                db.SaveChanges();
            }

            /// <summary>One home club dual flight on each of the first <paramref name="days"/> days of the month.</summary>
            public void AddFlightsOverDays(FlightContext db, int year, int month, int days)
            {
                for (var day = 1; day <= days; day++)
                    AddFlight(db, new DateTime(year, month, day), Home, HomePilot, HomePilot2, HomePilot);
                db.SaveChanges();
            }

            public Flight AddFlight(FlightContext db, DateTime date, Location at, Pilot pilot, Pilot backseat, Pilot betaler)
            {
                var flight = new Flight
                {
                    FlightId = Guid.NewGuid(),
                    Date = date,
                    PlaneId = Plane.PlaneId,
                    PilotId = pilot.PilotId,
                    PilotBackseatId = backseat?.PilotId,
                    BetalerId = betaler.PilotId,
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
        }
    }
}
