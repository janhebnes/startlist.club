using System.Configuration;
using System.Net.Mail;
using FlightJournal.Web;
using FlightJournal.Web.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twilio;

namespace FlightJournal.Tests.ServiceProviders
{
    [TestClass]
    public class SmtpTest
    {
        //[Ignore] // The service costs money - the test does not need to run on each cycle
        [TestMethod]
        [TestCategory("ServiceProviders")]
        public void Smtp_Send()
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.To.Add(new MailAddress("jan.hebnes@gmail.com"));
                    mail.Subject = "Unit testing startlist.club";
                    mail.Body = "Hello world.";

                    smtpClient.Send(mail);
                }
            }
            Assert.Inconclusive("Smtp send without errors - validate email arrival.");
        }
    }
}
