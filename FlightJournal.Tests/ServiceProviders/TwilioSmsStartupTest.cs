using System;
using System.Configuration;
using FakeItEasy;
using FlightJournal.Web.Configuration;
using FlightJournal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.ServiceProviders
{
    /// <summary>
    /// The SMS configuration is resolved once per app start, logged once without values, and the
    /// PhoneCode two-factor provider stays registered regardless. Injected values only; obviously fake.
    /// </summary>
    [TestClass]
    public class TwilioSmsStartupTest
    {
        private const string AccountSid = "AC00000000000000000000000000000000";
        private const string AuthToken = "fake-auth-token";
        private const string KeySid = "SK00000000000000000000000000000000";
        private const string KeySecret = "fake-key-secret";
        private const string FromNumber = "+4600000000";

        [TestCleanup]
        public void Cleanup()
        {
            Settings.ResetTwilioSms();
        }

        private static void AssertNoValues(string line)
        {
            foreach (var secret in new[] { AccountSid, AuthToken, KeySid, KeySecret, FromNumber })
                Assert.IsFalse(line.Contains(secret), "The startup line must not contain a configured value");
        }

        [TestMethod]
        public void Describe_Regional_ModeAndRegionOnly()
        {
            var line = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret).Describe();

            StringAssert.StartsWith(line, "SMS login: mode=RegionalApiKey");
            StringAssert.Contains(line, "region=ie1");
            AssertNoValues(line);
        }

        [TestMethod]
        public void Describe_Legacy_IsUs1()
        {
            var line = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, null, null, null, null).Describe();

            StringAssert.StartsWith(line, "SMS login: mode=LegacyUs1AuthToken");
            StringAssert.Contains(line, "region=us1");
            AssertNoValues(line);
        }

        [TestMethod]
        public void Describe_Misconfigured_NamesTheSettings_NotTheirValues()
        {
            var line = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, null).Describe();

            StringAssert.StartsWith(line, "SMS login: mode=Misconfigured");
            StringAssert.Contains(line, "TwilioApiKeySecret");
            AssertNoValues(line);
        }

        [TestMethod]
        public void Describe_NotConfigured()
        {
            StringAssert.StartsWith(TwilioSmsConfig.Resolve(null, null, null, null, null, null, null).Describe(), "SMS login: mode=NotConfigured");
        }

        private static string DescribeOf(string accountSid, string authToken, string fromNumber,
                                         string region, string edge, string apiKeySid, string apiKeySecret)
        {
            var line = TwilioSmsConfig.Resolve(accountSid, authToken, fromNumber, region, edge, apiKeySid, apiKeySecret).Describe();
            AssertNoValues(line);
            return line;
        }

        [TestMethod]
        public void Describe_NotConfigured_DemoCombination_NamesMissingRegionalSettings()
        {
            // Region, edge and sender number set: a regional setup, missing the account SID and the API key
            StringAssert.Contains(DescribeOf(null, null, FromNumber, "ie1", "dublin", null, null),
                "missing=TwilioAccountSid,TwilioApiKeySid,TwilioApiKeySecret");
        }

        [TestMethod]
        public void Describe_NotConfigured_NothingSet_NamesTheRecommendedRegionalSettings()
        {
            StringAssert.Contains(DescribeOf(null, null, null, null, null, null, null),
                "missing=TwilioAccountSid,TwilioFromNumber,TwilioRegion,TwilioApiKeySid,TwilioApiKeySecret");
        }

        [TestMethod]
        public void Describe_NotConfigured_LegacyAttempt_NamesMissingLegacySettings()
        {
            // Only the auth token points to the legacy US1 mode
            StringAssert.Contains(DescribeOf(null, AuthToken, FromNumber, null, null, null, null), "missing=TwilioAccountSid");
            StringAssert.Contains(DescribeOf(AccountSid, AuthToken, "  ", null, null, null, null), "missing=TwilioFromNumber");
        }

        [TestMethod]
        public void Describe_Misconfigured_NamesMissingSettings()
        {
            StringAssert.Contains(DescribeOf(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, null), "missing=TwilioApiKeySecret");
            StringAssert.Contains(DescribeOf(AccountSid, AuthToken, FromNumber, "au1", null, KeySid, KeySecret), "missing=TwilioEdge");
            StringAssert.Contains(DescribeOf(AccountSid, AuthToken, FromNumber, null, null, KeySid, KeySecret), "missing=TwilioRegion");
        }

        [TestMethod]
        public void Describe_Sending_HasNoMissingPart()
        {
            Assert.IsFalse(DescribeOf(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret).Contains("missing="));
            Assert.IsFalse(DescribeOf(AccountSid, AuthToken, FromNumber, null, null, null, null).Contains("missing="));
        }

        [TestMethod]
        public void TwilioSms_IsResolvedOnce_ThenReused()
        {
            var calls = 0;
            Settings.ResetTwilioSms(() =>
            {
                calls++;
                return TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret);
            });

            var first = Settings.TwilioSms;
            var second = Settings.TwilioSms;

            Assert.AreSame(first, second);
            Assert.AreEqual(1, calls, "Resolved once, then the same instance everywhere");
        }

        [TestMethod]
        public void TwilioSms_UnreadableSettings_IsMisconfigured_NotAnError()
        {
            // Resolving now happens at startup: a configuration error must hide SMS, not stop the site
            Settings.ResetTwilioSms(() => throw new ConfigurationErrorsException("Missing ServiceCredentials section"));

            var config = Settings.TwilioSms;

            Assert.AreEqual(TwilioSmsMode.Misconfigured, config.Mode);
            Assert.IsFalse(config.CanSend);
        }

        [TestMethod]
        public void TwilioSms_AnyResolveError_IsMisconfigured_WithTypeNameOnly()
        {
            // Not only configuration errors: any failure while resolving makes SMS unavailable instead of failing every access
            Settings.ResetTwilioSms(() => throw new InvalidOperationException("detail with " + FromNumber));

            var config = Settings.TwilioSms;

            Assert.AreEqual(TwilioSmsMode.Misconfigured, config.Mode);
            Assert.IsFalse(config.CanSend);
            var line = config.Describe();
            StringAssert.Contains(line, "InvalidOperationException");
            Assert.IsFalse(line.Contains("detail with"), "The exception message is never logged");
            AssertNoValues(line);
        }

        [TestMethod]
        public void StartupLine_WhenResolvingThrows_LogsTypeNameOnly_AndDoesNotThrow()
        {
            var line = FlightJournal.Web.Startup.SmsLoginStartupLine(() => throw new InvalidOperationException("detail with " + FromNumber));

            StringAssert.StartsWith(line, "SMS login:");
            StringAssert.Contains(line, "InvalidOperationException");
            Assert.IsFalse(line.Contains("detail with"), "The exception message is never logged");
            AssertNoValues(line);
        }

        [TestMethod]
        public void StartupLine_Normally_IsTheDescription()
        {
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret);

            Assert.AreEqual(config.Describe(), FlightJournal.Web.Startup.SmsLoginStartupLine(() => config));
        }

        [TestMethod]
        public void PhoneCodeProvider_IsAlwaysRegistered_EvenWhenSmsCannotSend()
        {
            // PhoneCode must stay registered unconditionally. If it were only registered when SMS can send,
            // ASP.NET Identity would no longer see it as a valid second factor and would skip two-factor
            // authentication for users whose only second factor is SMS. SMS is hidden in the UI instead.
            Settings.ResetTwilioSms(() => TwilioSmsConfig.Resolve(null, null, null, null, null, null, null));
            Assert.IsFalse(Settings.TwilioSms.CanSend);
            var manager = new ApplicationUserManager(A.Fake<IUserStore<ApplicationUser>>());

            ApplicationUserManager.RegisterTwoFactorProviders(manager);

            Assert.IsTrue(manager.TwoFactorProviders.ContainsKey("PhoneCode"));
            Assert.IsTrue(manager.TwoFactorProviders.ContainsKey("EmailCode"));
        }
    }
}
