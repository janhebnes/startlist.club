using System.Net.Mime;
using System.Security.Principal;
using System.Web;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Models;
using FlightJournal.Web.Validators;
using Microsoft.AspNet.Identity;

namespace FlightJournal.Web.Extensions
{
    public static class IPrincipalExtension
    {
        /// <summary>
        /// System administrators can create clubs and mange the whole system 
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        /// <example> instead of using Request.RequestContext.HttpContext.User.IsInRole("Admin") you can now User.IsAdministrator </example>
        public static bool IsAdministrator(this IPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
                return false;

            return principal.IsInRole("Administrator");
        }

        /// <summary>
        /// System managers can manage club pilots create planes and create locations / validates the role is assigned and the active club is the pilots club, includes Admin Roles as managers
        /// </summary>
        /// <param name="principal"></param>
        /// <example>Allows the use of User.IsManager</example>
        /// <returns></returns>
        public static bool IsManager(this IPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
                return false;

            if (principal.IsInRole("Administrator"))
                return true;

            return (principal.IsInRole("Manager") 
                && (PilotController.CurrentUserPilot.ClubId > 0)
                && (PilotController.CurrentUserPilot.ClubId == ClubController.CurrentClub.ClubId)); 
        }

        /// <summary>
        /// System editors can edit flights without time restrictions, includes Manager Roles and Admin Roles as Editors. 
        /// </summary>
        /// <param name="principal"></param>
        /// <example>Allows the use of User.IsEditor</example>
        /// <returns></returns>
        public static bool IsEditor(this IPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
                return false;

            if (principal.IsInRole("Administrator"))
                return true;

            return ((principal.IsInRole("Manager") || principal.IsInRole("Editor"))
                && (PilotController.CurrentUserPilot.ClubId > 0)
                && (PilotController.CurrentUserPilot.ClubId == ClubController.CurrentClub.ClubId)); 
        }

        /// <summary>
        /// Validates if the visiting request is bound to a pilot profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <example>in razor you can simply ask User.IsPilot</example>
        public static bool IsPilot(this IPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
                return false;

            return (PilotController.CurrentUserPilot != null
                && PilotController.CurrentUserPilot.PilotId > 0);
        }

        /// <summary>
        /// Returns the active pilot profile bound to the authenticated user session
        /// </summary>
        /// <param name="request"></param>
        /// <example>Allows the use of User.Pilot</example>
        /// <returns></returns>
        public static Pilot Pilot(this IPrincipal principal)
        {
            if (principal == null)
                return null;

            return PilotController.CurrentUserPilot;
        }

        /// <summary>
        /// Returns the ApplocationUser Profile of the current user logged on the system 
        /// </summary>
        /// <param name="principal"></param>
        /// <example>Allows the use of User>.Profile</example>
        /// <returns></returns>
        public static ApplicationUser Profile(this IPrincipal principal)
        {
            if (principal == null)
                return null;

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var user = db.Users.Find(principal.Identity.GetUserId());
                return user;
            }
        }

        /// <summary>
        /// Profilen kan enten være en mobil telefon token login eller en normal email bruger login
        /// Mobil telefon login har ikke nogen profil visning... 
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static bool IsMobilProfile(this IPrincipal principal)
        {
            return MobilNumberValidator.IsValid(principal.Identity.Name);
        }
    }
}
