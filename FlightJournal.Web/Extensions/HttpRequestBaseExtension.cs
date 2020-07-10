using System.Web;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Extensions
{
    public static class HttpRequestBaseExtension
    {
        /// <summary>
        /// Validates if the visiting request is bound to a pilot profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <example>in razor you can simply ask Request.IsPilot</example>
        public static bool IsPilot(this HttpRequestBase request)
        {
            if (request == null)
                return false;

            return (PilotController.CurrentUserPilot != null 
                && PilotController.CurrentUserPilot.PilotId > 0);
        }
        /// <summary>
        /// Validates if the visiting request is bound to an instructor profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <example>in razor you can simply ask Request.IsPilot</example>
        public static bool IsInstructor(this HttpRequestBase request)
        {
            return request?.IsPilot() ?? false; // TODO: implement
        }

        /// <summary>
        /// Returns the active pilot profile bound to the authenticated user session
        /// </summary>
        /// <param name="request"></param>
        /// <example>Allows the user of Request.Pilot</example>
        /// <returns></returns>
        public static Pilot Pilot(this HttpRequestBase request)
        {
            if (request == null)
                return null;

            return PilotController.CurrentUserPilot;
        }

        /// <summary>
        /// Validates if a club is active based on the route filter information of the current request
        /// </summary>
        /// <param name="request"></param>
        /// <example>Allows the user of Request.IsClub</example>
        /// <returns></returns>
        public static bool IsClub(this HttpRequestBase request)
        {
            if (request == null)
                return false;
            
            return (ClubController.CurrentClub != null 
                && ClubController.CurrentClub.ClubId > 0);
        }

        /// <summary>
        /// Returns the Club that is active based on the route filter information of the current request
        /// </summary>
        /// <param name="request"></param>
        /// <example>Allows the user of Request.Club</example>
        /// <returns></returns>
        public static Club Club(this HttpRequestBase request)
        {
            if (request == null)
                return null;

            return ClubController.CurrentClub;
        }

        /// <summary>
        /// Help return the current language selected in the browser
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string LanguageCode(this HttpRequestBase request)
        {
            return Translations.Internationalization.LanguageCode;
        }

        /// <summary>
        /// Help return the default mobile prefix for the selected UI language
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string PhoneNumberInternationalPrefix(this HttpRequestBase request)
        {
            string lang = Translations.Internationalization.LanguageCode;
            return Web.Validators.MobilNumberValidator.FindPhoneNumberInternationalPrefix(lang);
            
        }
    }
}