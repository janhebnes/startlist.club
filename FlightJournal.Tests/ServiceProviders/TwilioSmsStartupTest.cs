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
