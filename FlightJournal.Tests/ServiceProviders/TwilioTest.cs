﻿using System.Configuration;
using FlightJournal.Web.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
//using Twilio;

namespace FlightJournal.Tests.ServiceProviders
{
    [TestClass]
    public class TwilioTest
    {
        [Ignore] // The service costs money - the test does not need to run on each cycle
        [TestMethod]
        [TestCategory("ServiceProviders")]
        public void Twilio_Send_SMS_with_API()
        {
            Assert.Inconclusive("Not active on normal test cycle");
            //var settings = ConfigurationManager.GetSection("serviceCredentials") as ServiceCredentialsConfigurationSection;
            //if (settings == null)
            //    throw new ConfigurationErrorsException("Missing ServiceCredentials section");

            //if (!string.IsNullOrWhiteSpace(settings.TwilioAccountSid)
            //    && !string.IsNullOrWhiteSpace(settings.TwilioAuthToken)
            //    && !string.IsNullOrWhiteSpace(settings.TwilioFromNumber))
            //{

            //    // Find your Account Sid and Auth Token at twilio.com/user/account 
            //    TwilioClient.Init(Settings.TwilioAccountSid, Settings.TwilioAuthToken);

            //    var smsmessage = MessageResource.Create(
            //        "+4524250682",
            //        from: Settings.TwilioFromNumber,
            //        body: "Twilio Testing account setup for startlist.club"
            //    );

            //    Assert.IsNotNull(smsmessage.Sid);
            //    Assert.IsNull(smsmessage.ErrorMessage);
            //}
            //else
            //{
            //    Assert.Fail("Configuration not set");
            //}
        }
    }
}
