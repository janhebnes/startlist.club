using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FlightJournal.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SendGrid;

namespace FlightJournal.Tests.ServiceProviders
{
    [TestClass]
    
    public class SendGridTest
    {
        [Ignore] // The service costs money - the test does not need to run on each cycle
        [TestMethod]
        [TestCategory("ServiceProviders")]
        public void SendGrid_Send_Mail_with_API()
        {
            var settings = ConfigurationManager.GetSection("serviceCredentials") as ServiceCredentialsConfigurationSection;
            if (settings == null)
                throw new ConfigurationErrorsException("Missing ServiceCredentials section");

            if (!string.IsNullOrWhiteSpace(settings.TwilioAccountSid)
                && !string.IsNullOrWhiteSpace(settings.TwilioAuthToken)
                && !string.IsNullOrWhiteSpace(settings.TwilioFromNumber))
            {
                // Create the email object first, then add the properties.
                SendGridMessage myMessage = new SendGridMessage();
                myMessage.AddTo("jan.hebnes@gmail.com");
                myMessage.From = new MailAddress(settings.SendGridFromEmail, settings.SendGridFromName);
                myMessage.Subject = "SendGrid Testing account setup for startlist.club";
                myMessage.Text = "Hello World";

                // Create credentials, specifying your user name and password.
                var credentials = new NetworkCredential(settings.SendGridUserName, settings.SendGridPassword);

                // Create an Web transport for sending email.
                var transportWeb = new SendGrid.Web(credentials);

                // Send the email.
                transportWeb.Deliver(myMessage);

                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("Configuration not set");
            }
        }
    }
}
