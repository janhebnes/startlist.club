using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyntheticReportData = FlightJournal.Tests.Integration.ReportLoadTest.SyntheticReportData;

namespace FlightJournal.Tests.Integration
{
    /// <summary>
    /// The year/month CSV export and the day JSON export (GitHub #281). Runs against the LocalDB development database.
    /// Every test works inside a TransactionScope that is never completed, so all synthetic data is rolled back.
    /// "Legacy" is the export as it was: a DATEPART query, OrderBy(Departure), the club filter in memory, and string concatenation.
    /// </summary>
    [TestClass]
    public class ReportExportTest
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

        /// <summary>Forget tracked entities, so later reads go to the database (as a fresh request would).</summary>
        private void Detach()
        {
            foreach (var entry in _db.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }

        private List<Flight> Load(ReportPeriod period, ReportClubFilter club)
        {
            Detach();
            _counter.Reset();
            return ReportExport.LoadFlights(_db, period, club);
        }

        /// <summary>The export query as it was: DATEPART predicate, OrderBy(Departure), club filter in memory with lazy loads.</summary>
        private List<Flight> LegacyFlights(Expression<Func<Flight, bool>> datePredicate, ReportClubFilter club)
        {
            Detach();
            return _db.Flights.Where(datePredicate).Where(f => f.Deleted == null).OrderBy(o => o.Departure).ToList()
                .Where(club.IsClubFlight).ToList();
        }

        private static string LegacySafeCsv(string input)
        {
            var csv = input.Replace("æ", "ae");
            csv = csv.Replace("Æ", "AE");
            csv = csv.Replace("ø", "oe");
            csv = csv.Replace("Ø", "OE");
            csv = csv.Replace("å", "aa");
            csv = csv.Replace("Å", "AA");
            csv = csv.Replace("é", "e");
            return csv;
        }

        private static string LegacyCsv(IEnumerable<Flight> flights)
        {
            return flights.Aggregate(LegacySafeCsv(Flight.CsvHeaders), (current, flight) => current + LegacySafeCsv(flight.ToCsvString()));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Export_Year_OneCommand_DateRange_ConstantInFlights()
        {
            var data = SyntheticReportData.Create(_db);
            var counts = new List<int>();
            var added = 0;
            foreach (var total in new[] { 5, 45 })
            {
                data.AddFlightsOverYear(_db, Year, total - added);
                added = total;

                var flights = Load(ReportPeriod.ForYear(Year), data.HomeFilter);
                var csv = ReportExport.ToCsv(flights);

                Assert.AreEqual(total, flights.Count);
                Assert.IsTrue(csv.Length > 0);
                counts.Add(_counter.Count);
                Assert.AreEqual(1, _counter.Count, $"N={total}:\n" + string.Join("\n---\n", _counter.Commands));
                var sql = _counter.Commands.Single();
                StringAssert.Contains(sql, "[Date] >= ");
                Assert.IsFalse(sql.Contains("DATEPART"), "The export must use an index-friendly date range:\n" + sql);
            }
            Console.WriteLine("Year export SQL commands: N=5 -> " + counts[0] + ", N=45 -> " + counts[1]);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Export_NoLazyLoads_AfterLoad()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 5, 1);
            data.AddFlight(_db, date, data.Home, data.HomePilot, data.AwayPilot, data.HomePilot);
            data.AddFlight(_db, date, data.Away, data.AwayPilot, data.HomePilot2, data.HomePilot2);
            data.AddFlight(_db, date, data.Home, data.HomePilot3, null, data.HomePilot3);
            _db.SaveChanges();

            var flights = Load(ReportPeriod.ForDay(date), data.HomeFilter);
            var touched = flights.Select(f => f.ToCsvString() + new JavaScriptSerializer().Serialize(f.ToJsonObject())).ToList();

            Assert.AreEqual(3, touched.Count);
            Assert.AreEqual(1, _counter.Count, "Only the load itself; no lazy loads:\n" + string.Join("\n---\n", _counter.Commands));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Export_ClubPeopleFilter_MatchesLegacy_ExcludesDeleted()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 6, 1);
            var homePilotAway = data.AddFlight(_db, date, data.Away, data.HomePilot, null, data.AwayPilot);
            var homeBackseatAway = data.AddFlight(_db, date, data.Away, data.AwayPilot, data.HomePilot, data.AwayPilot);
            var homePaysAway = data.AddFlight(_db, date, data.Away, data.AwayPilot, null, data.HomePilot);
            var awayAtHome = data.AddFlight(_db, date, data.Home, data.AwayPilot, data.AwayPilot2, data.AwayPilot);
            var deletedHome = data.AddFlight(_db, date, data.Home, data.HomePilot2, null, data.HomePilot2);
            deletedHome.Deleted = date;
            _db.SaveChanges();
            var dayPredicate = (Expression<Func<Flight, bool>>)(f => f.Date.Day == 1 && f.Date.Month == 6 && f.Date.Year == Year);

            var club = Load(ReportPeriod.ForDay(date), data.HomeFilter).Select(f => f.FlightId).ToList();
            var legacyClub = LegacyFlights(dayPredicate, data.HomeFilter).Select(f => f.FlightId).ToList();
            var unfiltered = Load(ReportPeriod.ForDay(date), ReportClubFilter.Unfiltered).Select(f => f.FlightId).ToList();
            var legacyUnfiltered = LegacyFlights(dayPredicate, ReportClubFilter.Unfiltered).Select(f => f.FlightId).ToList();

            CollectionAssert.AreEquivalent(new[] { homePilotAway.FlightId, homeBackseatAway.FlightId, homePaysAway.FlightId }, club,
                "Club people only: a foreign flight at the home field is not included (no location rule), deleted flights are excluded");
            CollectionAssert.AreEquivalent(legacyClub, club);
            CollectionAssert.AreEquivalent(new[] { homePilotAway.FlightId, homeBackseatAway.FlightId, homePaysAway.FlightId, awayAtHome.FlightId }, unfiltered);
            CollectionAssert.AreEquivalent(legacyUnfiltered, unfiltered);
        }

        /// <summary>Home club flights with distinct departures and Danish letters in the note, spread over two months.</summary>
        private void AddCsvFlights(SyntheticReportData data)
        {
            var notes = new[] { "Ræv Æble", "Sø Øst", "Å å", "Café é", "plain" };
            for (var i = 0; i < notes.Length; i++)
            {
                var date = new DateTime(Year, i < 3 ? 3 : 4, i + 1);
                var flight = data.AddFlight(_db, date, i % 2 == 0 ? data.Home : data.Away, data.HomePilot, i % 2 == 0 ? data.HomePilot2 : null, data.HomePilot);
                flight.Departure = date.AddHours(9 + i);
                flight.Landing = date.AddHours(10 + i);
                flight.Description = notes[i];
            }
            _db.SaveChanges();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Export_Csv_MatchesLegacyOutput_YearAndMonth()
        {
            var data = SyntheticReportData.Create(_db);
            AddCsvFlights(data);

            var year = ReportExport.ToCsv(Load(ReportPeriod.ForYear(Year), data.HomeFilter));
            var legacyYear = LegacyCsv(LegacyFlights(f => f.Date.Year == Year, data.HomeFilter));
            var month = ReportExport.ToCsv(Load(ReportPeriod.ForMonth(Year, 3), data.HomeFilter));
            var legacyMonth = LegacyCsv(LegacyFlights(f => f.Date.Month == 3 && f.Date.Year == Year, data.HomeFilter));

            Assert.AreEqual(legacyYear, year);
            Assert.AreEqual(legacyMonth, month);
            StringAssert.Contains(year, "Raev AEble");
            Assert.IsFalse(year.Contains("æ") || year.Contains("é"), "Danish letters are transliterated");
            Assert.AreEqual(6, year.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length, "Header plus 5 flights");
            Assert.AreEqual(4, month.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length, "Header plus 3 flights");
            CollectionAssert.AreEqual(new UTF8Encoding().GetBytes(legacyYear), new UTF8Encoding().GetBytes(year));
            Assert.AreNotEqual(0xEF, new UTF8Encoding().GetBytes(year)[0], "No byte order mark");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ExportJson_Day_MatchesLegacy()
        {
            var data = SyntheticReportData.Create(_db);
            AddCsvFlights(data);
            var day = new DateTime(Year, 3, 2);
            // A second flight on the same day with a later departure
            var second = data.AddFlight(_db, day, data.Away, data.AwayPilot, data.HomePilot, data.HomePilot3);
            second.Departure = day.AddHours(15);
            _db.SaveChanges();
            var serializer = new JavaScriptSerializer();

            var json = serializer.Serialize(Load(ReportPeriod.ForDay(day), data.HomeFilter).Select(f => f.ToJsonObject()).ToList());
            var legacyJson = serializer.Serialize(LegacyFlights(f => f.Date.Day == 2 && f.Date.Month == 3 && f.Date.Year == Year, data.HomeFilter)
                .Select(f => f.ToJsonObject()).ToList());

            Assert.AreEqual(legacyJson, json);
            StringAssert.Contains(json, second.FlightId.ToString());
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Export_Order_DepartureThenFlightId_Deterministic()
        {
            var data = SyntheticReportData.Create(_db);
            var date = new DateTime(Year, 7, 1);
            var sameA = data.AddFlight(_db, date, data.Home, data.HomePilot, null, data.HomePilot);
            var sameB = data.AddFlight(_db, date, data.Home, data.HomePilot2, null, data.HomePilot2);
            var noDeparture = data.AddFlight(_db, date, data.Home, data.HomePilot3, null, data.HomePilot3);
            sameA.Departure = date.AddHours(11);
            sameB.Departure = date.AddHours(11);
            noDeparture.Departure = null;
            _db.SaveChanges();
            var ids = new[] { sameA.FlightId, sameB.FlightId, noDeparture.FlightId };
            Detach();
            // SQL Server orders uniqueidentifier differently from .NET, so the expectation is evaluated in SQL
            var expected = _db.Flights.Where(f => ids.Contains(f.FlightId))
                .OrderBy(f => f.Departure).ThenBy(f => f.FlightId).Select(f => f.FlightId).ToList();

            var first = Load(ReportPeriod.ForDay(date), data.HomeFilter).Select(f => f.FlightId).ToList();
            var again = Load(ReportPeriod.ForDay(date), data.HomeFilter).Select(f => f.FlightId).ToList();

            CollectionAssert.AreEqual(expected, first);
            CollectionAssert.AreEqual(first, again);
        }

        [TestMethod]
        public void TryGetPeriod_Valid_ReturnsYearMonthDay()
        {
            Assert.IsTrue(ReportExport.TryGetPeriod(2020, null, null, out var year));
            Assert.AreEqual(ReportPeriodKind.Year, year.Kind);
            Assert.AreEqual(new DateTime(2020, 1, 1), year.Start);
            Assert.AreEqual(new DateTime(2021, 1, 1), year.EndExclusive);

            Assert.IsTrue(ReportExport.TryGetPeriod(2020, 2, null, out var month));
            Assert.AreEqual(ReportPeriodKind.Month, month.Kind);
            Assert.AreEqual(new DateTime(2020, 2, 1), month.Start);
            Assert.AreEqual(new DateTime(2020, 3, 1), month.EndExclusive);

            Assert.IsTrue(ReportExport.TryGetPeriod(2024, 2, 29, out var day));
            Assert.AreEqual(ReportPeriodKind.Day, day.Kind);
            Assert.AreEqual(new DateTime(2024, 2, 29), day.Start);
            Assert.AreEqual(new DateTime(2024, 3, 1), day.EndExclusive);
        }

        [TestMethod]
        public void TryGetPeriod_Invalid_ReturnsFalse()
        {
            var invalid = new (int year, int? month, int? day)[]
            {
                (2020, 0, null), (2020, 13, null), (2020, 1, 0), (2020, 1, 32), (2023, 2, 29),
                (0, null, null), (9999, null, null), (2020, null, 5)
            };
            foreach (var (year, month, day) in invalid)
            {
                Assert.IsFalse(ReportExport.TryGetPeriod(year, month, day, out var period), $"{year}-{month}-{day}");
                Assert.IsNull(period);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Export_InvalidMonth_Returns404_WithoutQuery()
        {
            _counter.Reset();
            using (var controller = new ReportController())
            {
                var result = controller.Export(2020, 13);

                Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
            }
            Assert.AreEqual(0, _counter.Count, string.Join("\n---\n", _counter.Commands));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ExportJson_InvalidDay_Returns404_WithoutQuery()
        {
            _counter.Reset();
            using (var controller = new ReportController())
            {
                var result = controller.ExportJson(2023, 2, 30);

                Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
            }
            Assert.AreEqual(0, _counter.Count, string.Join("\n---\n", _counter.Commands));
        }
    }
}
