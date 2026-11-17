using FlightJournal.Web.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twilio.Exceptions;

namespace FlightJournal.Tests.ServiceProviders
{
    /// <summary>
    /// How the app talks to Twilio for SMS. Pure: no network, no ConfigurationManager.
    /// Obviously fake values only; never real SIDs, secrets or phone numbers.
    /// </summary>
    [TestClass]
    public class TwilioSmsConfigTest
    {
        private const string AccountSid = "AC00000000000000000000000000000000";
        private const string AuthToken = "fake-auth-token";
        private const string KeySid = "SK00000000000000000000000000000000";
        private const string KeySecret = "fake-key-secret";
        private const string FromNumber = "+4600000000";

        [TestMethod]
        public void Regional_Ie1_UsesApiKey_WithAccountSid()
        {
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, " IE1 ", "dublin", KeySid, KeySecret);

            Assert.AreEqual(TwilioSmsMode.RegionalApiKey, config.Mode);
            Assert.AreEqual(KeySid, config.Username);
            Assert.AreEqual(KeySecret, config.Password);
            Assert.AreEqual(AccountSid, config.AccountSid);
            Assert.AreEqual("ie1", config.Region);
            Assert.AreEqual("dublin", config.Edge);
            Assert.AreEqual(FromNumber, config.FromNumber);
        }

        [TestMethod]
        public void Regional_Ie1_WithoutEdge_DefaultsToDublin()
        {
            // A region without an edge routes to US1, so ie1 must get its edge
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "", KeySid, KeySecret);

            Assert.AreEqual(TwilioSmsMode.RegionalApiKey, config.Mode);
            Assert.AreEqual("dublin", config.Edge);
        }

        [TestMethod]
        public void Regional_OtherRegionWithoutEdge_Misconfigured()
        {
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "au1", "", KeySid, KeySecret);

            Assert.AreEqual(TwilioSmsMode.Misconfigured, config.Mode);
            StringAssert.Contains(config.Problem, "TwilioEdge");
        }

        [TestMethod]
        public void Regional_MissingKeySecret_Misconfigured_EvenWithAuthToken()
        {
            // No silent US1 fallback: it would only produce 21663 again
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, null);

            Assert.AreEqual(TwilioSmsMode.Misconfigured, config.Mode);
            Assert.IsNull(config.Password);
        }

        [TestMethod]
        public void ApiKeyWithoutRegion_Misconfigured()
        {
            // A key is only valid in the region it was created in
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, null, null, KeySid, KeySecret);

            Assert.AreEqual(TwilioSmsMode.Misconfigured, config.Mode);
            StringAssert.Contains(config.Problem, "TwilioRegion");
        }

        [TestMethod]
        public void NoRegion_AuthToken_IsLegacyUs1()
        {
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, null, null, null, null);

            Assert.AreEqual(TwilioSmsMode.LegacyUs1AuthToken, config.Mode);
            Assert.AreEqual(AccountSid, config.Username);
            Assert.AreEqual(AuthToken, config.Password);
            Assert.IsNull(config.Region);
            Assert.IsNull(config.Edge);
        }

        [TestMethod]
        public void MissingAccountSidOrFromNumber_NotConfigured()
        {
            Assert.AreEqual(TwilioSmsMode.NotConfigured,
                TwilioSmsConfig.Resolve(null, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret).Mode);
            Assert.AreEqual(TwilioSmsMode.NotConfigured,
                TwilioSmsConfig.Resolve(AccountSid, AuthToken, " ", "ie1", "dublin", KeySid, KeySecret).Mode);
        }

        [TestMethod]
        public void CreateClient_Regional_HasRegionEdgeAndAccountSid()
        {
            // Constructing the client makes no request
            var client = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret).CreateClient();

            Assert.AreEqual("ie1", client.Region);
            Assert.AreEqual("dublin", client.Edge);
            Assert.AreEqual(AccountSid, client.AccountSid);
        }

        [TestMethod]
        public void CreateClient_Legacy_HasNoRegionOrEdge()
        {
            var client = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, null, null, null, null).CreateClient();

            Assert.IsNull(client.Region);
            Assert.IsNull(client.Edge);
            Assert.AreEqual(AccountSid, client.AccountSid);
        }

        [TestMethod]
        public void SmsDeliveryException_FromApiException_CarriesCodes_NotTwilioText()
        {
            var config = TwilioSmsConfig.Resolve(AccountSid, AuthToken, FromNumber, "ie1", "dublin", KeySid, KeySecret);
            var twilioError = new ApiException(21663, 400, "From +46000 routing configuration is incorrect", null, null, null);

            var ex = SmsDeliveryException.From(twilioError, config);

            Assert.AreEqual(21663, ex.TwilioCode);
            Assert.AreEqual(400, ex.TwilioStatus);
            Assert.AreEqual(TwilioSmsMode.RegionalApiKey, ex.Mode);
            Assert.AreEqual("ie1", ex.Region);
            StringAssert.Contains(ex.Message, "21663");
            Assert.IsFalse(ex.Message.Contains("+46000"), "The message must not carry the phone number");
            Assert.IsFalse(ex.Message.Contains("routing"), "The message must not carry Twilio's text");
            Assert.IsNull(ex.InnerException, "Twilio's text (which can contain the From number) is not kept, so it never reaches telemetry");
        }
    }
}
