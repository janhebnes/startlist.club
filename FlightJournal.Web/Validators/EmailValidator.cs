using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Validators
{
    public class EmailValidator
    {
        public static bool IsValid(string email)
        {
            return IsValid(email, false);
        }

        public static bool IsValid(string email, bool isExistingPilot)
        {
            var cleanEmail = ParseEmail(email);
            var valid = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(cleanEmail);
                if (addr.Address == cleanEmail)
                {
                    valid = true;
                }
            }
            catch
            {
                return false;
            }

            if (valid)
            {
                if (!isExistingPilot)
                {
                    return true;
                }
                else
                {
                    using (var shortDb = new FlightContext())
                    {
                        return shortDb.Pilots.Any(d => d.Email == cleanEmail);
                    }
                }
            }
            return false;
        }

        public static List<Pilot> FindPilotsByEmail(string email)
        {
            email = ParseEmail(email);
            using (var shortDb = new FlightContext())
            {
                return shortDb.Pilots.Where(d => d.Email == email).ToList();
            }
        }

        public static string ParseEmail(string email)
        {
            if (email == null)
            {
                return string.Empty;
            }

            // Remove spaces (both trailing and inline)
            var n = email.Replace(" ", "");

            // Force lowercase
            n = n.ToLowerInvariant();

            return n;
        }
    }
}