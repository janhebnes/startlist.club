using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Configuration
{
    /// <summary>
    /// The retired OGN flight log integration (client package, connection string, views) stays removed. The live APRS/OGN
    /// tracking is a different feature and isn't covered here. Pure file and reflection checks, no network.
    /// </summary>
    [TestClass]
    public class NoOgnFlightLogTest
    {
        private static string RepositoryRoot =>
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

        private static string Web(params string[] parts) =>
            Path.Combine(new[] { RepositoryRoot, "FlightJournal.Web" }.Concat(parts).ToArray());

        private static string Read(string path)
        {
            Assert.IsTrue(File.Exists(path), "Missing file: " + path);
            return File.ReadAllText(path);
        }

        [TestMethod]
        public void WebAssembly_DoesNotReference_OgnFlightLogClient()
        {
            var references = typeof(FlightJournal.Web.MvcApplication).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();

            CollectionAssert.DoesNotContain(references, "OGN.FlightLog.Client");
        }

        [TestMethod]
        public void ConfigFiles_HaveNoOgnFlightLogConnectionString()
        {
            var files = new[]
            {
                Web("Web.config"), Web("Web.Release.config"), Web("Web.Dev.config"), Web("Web.Demo.config"),
                Path.Combine(RepositoryRoot, "FlightJournal.Tests", "App.config")
            };

            foreach (var file in files)
            {
                Assert.IsTrue(File.Exists(file), "Missing file: " + file);
                var xml = new XmlDocument();
                xml.Load(file); // comments are not elements, so a commented-out entry wouldn't count
                var entries = xml.SelectNodes("//connectionStrings/add[@name='OGN.FlightLog']");
                Assert.AreEqual(0, entries.Count, file);
            }
        }

        [TestMethod]
        public void PublishProfiles_HaveNoOgnFlightLogEntries()
        {
            var profiles = Directory.GetFiles(Web("Properties", "PublishProfiles"), "*.pubxml");

            Assert.IsTrue(profiles.Length > 0, "No publish profiles found");
            foreach (var profile in profiles)
                Assert.IsFalse(File.ReadAllText(profile).Contains("OGN.FlightLog"), profile);
        }

        [TestMethod]
        public void ProjectFiles_HaveNoOgnFlightLogPackageOrFiles()
        {
            var forbidden = new[] { "OGN.FlightLog.Client", "OGNFlightExtension", "_ognFlightLog", "OpenGliderNetworkFlightLogMonitor" };

            foreach (var file in new[] { Web("packages.config"), Web("FlightJournal.Web.csproj") })
            {
                var text = Read(file);
                foreach (var term in forbidden)
                    Assert.IsFalse(text.Contains(term), $"{Path.GetFileName(file)} still contains {term}");
            }
        }

        [TestMethod]
        public void Views_ReferenceNoOgnFlightLogTypes()
        {
            // Razor views compile at runtime, so a view still naming a removed type builds but fails when it's opened
            var forbidden = new[] { "OGN.FlightLog", "_ognFlightLog", "OgnFlightLog", "ognFlightLogAction" };
            var views = Directory.GetFiles(Web("Views"), "*.cshtml", SearchOption.AllDirectories);

            Assert.IsTrue(views.Length > 0, "No views found");
            var hits = views
                .SelectMany(v => forbidden.Where(File.ReadAllText(v).Contains).Select(t => v.Substring(RepositoryRoot.Length) + ": " + t))
                .ToList();
            Assert.AreEqual(0, hits.Count, string.Join(Environment.NewLine, hits));
        }
    }
}
