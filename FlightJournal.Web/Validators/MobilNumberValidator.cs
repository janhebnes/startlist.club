using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Models;
using FlightJournal.Web.Extensions;
using System;

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
            var valid = false;
            int num = 0;
            valid = (cleanNumber.StartsWith("+")    // prefixed with internationational code
                & int.TryParse(cleanNumber.Substring(1), out num)   // Numeric 
                & cleanNumber.Length >= 11);    // Minimum length
            if (cleanNumber.StartsWith("+45"))
            {
                // Danish number format validation
                valid = (cleanNumber.Length == 11);
            }
            else if (cleanNumber.StartsWith("+46"))
            {
                // Swedish number format validation
                valid = (cleanNumber.Length == 12);
            }
            else if (cleanNumber.StartsWith("+47"))
            {
                // Norsk number format validation
                valid = (cleanNumber.Length == 11);
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
                        return shortDb.Pilots.Any(d => d.MobilNumber == cleanNumber);
                    }
                }
            }
            return false;
        }

        public static List<Pilot> FindPilotsByMobilNumber(string mobilNumber)
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
            if (n.StartsWith("00"))
            {
                n = "+" + n.Substring(2);
            }

            // Add potential missing use of international phone code
            if (n.Length == 8)
            {
                //default based on language number
                string lang = Translations.Internationalization.LanguageCode;
                n = FindPhoneNumberInternationalPrefix(lang) + n;
            }

            return n;
        }

        /// <summary>
        /// Based on language iso code render phone number prefix
        /// </summary>
        /// <param name="lang">Language Code</param>
        /// <returns></returns>
        internal static string FindPhoneNumberInternationalPrefix(string lang)
        {
            if (lang == "no")
                return "+47";

            if (lang == "sv")
                return "+46";

            return "+45";
        }
    }
}