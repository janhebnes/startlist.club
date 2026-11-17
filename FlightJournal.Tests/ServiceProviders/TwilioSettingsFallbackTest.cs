using System;
using FlightJournal.Web.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.ServiceProviders
{
    /// <summary>
    /// Twilio settings come from the app setting first, then the App_Data ServiceCredentials fallback.
    /// Values are injected; no config file or ConfigurationManager. Obviously fake values only.
    /// </summary>
    [TestClass]
    public class TwilioSettingsFallbackTest
    {
        private static Func<string> MustNotBeRead => () => throw new AssertFailedException("The fallback must not be read when the app setting exists");

        [TestMethod]
        public void AppSettingWins_FallbackNotRead()
        {
            Assert.AreEqual("ie1", Settings.AppSettingOrFallback("ie1", MustNotBeRead));
        }

        [TestMethod]
        public void MissingAppSetting_UsesFallback()
        {
            Assert.AreEqual("dublin", Settings.AppSettingOrFallback(null, () => "dublin"));
        }

        [TestMethod]
        public void MissingInBothPlaces_IsBlank()
        {
            // The fallback section's attributes default to ""
            Assert.IsTrue(string.IsNullOrWhiteSpace(Settings.AppSettingOrFallback(null, () => "")));
            Assert.IsTrue(string.IsNullOrWhiteSpace(Settings.AppSettingOrFallback(null, () => null)));
        }

        [TestMethod]
        public void EmptyAppSetting_StillWins_LikeTheExistingTwilioSettings()
        {
            // Same rule as TwilioAccountSid/AuthToken/FromNumber: an app setting that exists, even empty, is used
            Assert.AreEqual("", Settings.AppSettingOrFallback("", MustNotBeRead));
        }

        [TestMethod]
        public void ServiceCredentialsSection_NewTwilioFields_DefaultEmpty()
        {
            // The constructor is private (only the configuration system creates the section)
            var section = (ServiceCredentialsConfigurationSection)Activator.CreateInstance(typeof(ServiceCredentialsConfigurationSection), nonPublic: true);

            Assert.AreEqual("", section.TwilioRegion);
            Assert.AreEqual("", section.TwilioEdge);
            Assert.AreEqual("", section.TwilioApiKeySid);
            Assert.AreEqual("", section.TwilioApiKeySecret);
        }

        [TestMethod]
        public void RegionalValuesFromFallbackOnly_CanSend_AndBothMissing_CannotSend()
        {
            string FromFallback(string value) => Settings.AppSettingOrFallback(null, () => value);

            var fromFallback = TwilioSmsConfig.Resolve(
                FromFallback("AC00000000000000000000000000000000"), FromFallback(""), FromFallback("+4600000000"),
                FromFallback("ie1"), FromFallback("dublin"), FromFallback("SK00000000000000000000000000000000"), FromFallback("fake-key-secret"));
            Assert.AreEqual(TwilioSmsMode.RegionalApiKey, fromFallback.Mode);
            Assert.IsTrue(fromFallback.CanSend);

            var missingEverywhere = TwilioSmsConfig.Resolve(
                FromFallback(""), FromFallback(""), FromFallback(""), FromFallback(""), FromFallback(""), FromFallback(""), FromFallback(""));
            Assert.IsFalse(missingEverywhere.CanSend);
        }
    }
}
