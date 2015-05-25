using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using FlightJournal.Web.Translations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Translations
{
    [TestClass]
    public class MessageTest
    {
        [TestInitialize]
        public void SetUp()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var solutionRoot = appPath.Remove(appPath.IndexOf("FlightJournal.Tests", StringComparison.InvariantCulture));
            var translationRoot = Path.Combine(solutionRoot, "FlightJournal.Web", "Translations");
            Messages.Instance.TranslationDirectoryFullPath = translationRoot;
        }

        [TestMethod]
        public void TranslationDirectoryFullPathIsTest()
        {
            Assert.AreNotEqual(Messages.Instance.TranslationDirectoryFullPath,"");
        }
        

        [TestMethod]
        public void SupportedLanguageIsoCodesCountIsTwo()
        {
            var iso = Messages.Instance.SupportedLanguageIsoCodes;
            Assert.IsNotNull(iso);
            Assert.IsTrue(iso.Count() == 2);
        }

        [TestMethod]
        public void SaveStore()
        {
            //var file = new Messages.TranslationFile() { Messages = new Messages.Translation[] { new Messages.Translation() { MsgId = "Overview", MsgStr = "Overblik"} } };
            //var langFullPath = Path.Combine(Messages.Instance.TranslationDirectoryFullPath, "da", "messages.xml");
            //var serializer = new XmlSerializer(typeof(Messages.TranslationFile));
            //using (StreamWriter writer = new StreamWriter(langFullPath))
            //{
            //    serializer.Serialize(writer,file);
            //}
            //Assert.IsNotNull(null);
        }
        
        
    }
}
