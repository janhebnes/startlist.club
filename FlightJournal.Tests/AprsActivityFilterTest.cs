using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightJournal.Web.Aprs;
using FlightJournal.Web.Configuration;
using FlightJournal.Web.Migrations.ApplicationDbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests
{
    [TestClass]
    public class AprsActivityFilterTest
    {

        [TestMethod]
        public void Simple()
        {
            var filter = new AprsActivityFilter();
            var t = new DateTime(2023, 6, 15, 12, 0, 0);
            var window = new Tuple<DateTime, DateTime>(new DateTime(2023, 6, 15, 11, 0, 0), new DateTime(2023, 6, 15, 13, 0, 0));
            var lati = 56;
            var longi = 12;
            var accepted = filter.ShouldAcceptFlight(t, window, lati, longi);
            Assert.IsTrue(accepted);
        }

        [TestMethod]
        public void UsingSettings_ShouldAccept()
        {
            var filter = new AprsActivityFilter();
            var t = DateTime.Now.Date + TimeSpan.FromHours(13);
            var window = Settings.AprsListenerActiveHours;
            var lati = 56;
            var longi = 12;
            var accepted = filter.ShouldAcceptFlight(t, window, lati, longi);
            Assert.IsTrue(accepted);
        }
        [TestMethod]
        public void UsingSettings_ShouldReject()
        {
            var filter = new AprsActivityFilter();
            var t = DateTime.Now.Date + TimeSpan.FromHours(7);
            var window = Settings.AprsListenerActiveHours;
            var lati = 56;
            var longi = 12;
            var accepted = filter.ShouldAcceptFlight(t, window, lati, longi);
            Assert.IsFalse(accepted);
        }

        [TestMethod]
        public void UsingSolar_ShouldReject()
        {
            var filter = new AprsActivityFilter();
            var t = DateTime.Now.Date + TimeSpan.FromHours(22.5);
            var window = new Tuple<DateTime, DateTime>( DateTime.Now.Date + TimeSpan.FromHours(1),  DateTime.Now.Date + TimeSpan.FromHours(23));
            var lati = 56;
            var longi = 12;
            var accepted = filter.ShouldAcceptFlight(t, window, lati, longi);
            Assert.IsFalse(accepted);
        }
        [TestMethod]
        public void UsingSolar_ShouldAccept()
        {
            var filter = new AprsActivityFilter();
            var t = DateTime.Now.Date + TimeSpan.FromHours(13);
            var window = new Tuple<DateTime, DateTime>( DateTime.Now.Date + TimeSpan.FromHours(1),  DateTime.Now.Date + TimeSpan.FromHours(23));
            var lati = 56;
            var longi = 12;
            var accepted = filter.ShouldAcceptFlight(t, window, lati, longi);
            Assert.IsTrue(accepted);
        }
    }
}
