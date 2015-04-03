using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Validators
{
    public class MobilNumberValidator
    {
        public static bool IsValid(string mobilNumber)
        {
            return IsValid(mobilNumber, false);
        }

        public static bool IsValid(string mobilNumber, bool isExistingPilot)
        {
            var cleanNumber = ParseMobilNumber(mobilNumber);
            var valid = (cleanNumber.StartsWith("+") & cleanNumber.Length == 11);
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
                        return shortDb.Pilots.Any(d => d.MobilNumber == cleanNumber);
                    }
                }
            }
            return false;
        }

        public static List<Pilot> GetPilots(string mobilNumber)
        {
            mobilNumber = ParseMobilNumber(mobilNumber);
            using (var shortDb = new FlightContext())
            {
                return shortDb.Pilots.Where(d => d.MobilNumber == mobilNumber).ToList();
            }
        }

        public static string ParseMobilNumber(string mobilNumber)
        {
            if (mobilNumber == null)
            {
                return string.Empty;
            }

            // Remove spaces
            var n = mobilNumber.Replace(" ", "");

            // Replace potential use of 00 instead of +
            if (n.Length == 12 && n.StartsWith("00"))
            {
                n = "+" + n.Substring(2);
            }

            // Add potential missing use of +45
            if (n.Length == 8)
            {
                n = "+45" + n;
            }

            return n;
        }
    }
}