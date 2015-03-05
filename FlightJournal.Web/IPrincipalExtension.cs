using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Models;

namespace FlightJournal.Web
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
            if (principal == null)
                return false;

            return principal.IsInRole("Administrator");
        }

        /// <summary>
        /// System managers can manage club pilots create planes and create locations / validates the role is assigned and the active club is the pilots club
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static bool IsManager(this IPrincipal principal)
        {
            if (principal == null)
                return false;

            return (principal.IsInRole("Manager") 
                && (PilotController.CurrentUserPilot.ClubId > 0) 
                && (PilotController.CurrentUserPilot.ClubId == ClubController.CurrentClub.ClubId));
        }

        /// <summary>
        /// System editors can edit flights without time restrictions
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static bool IsEditor(this IPrincipal principal)
        {
            if (principal == null)
                return false;

            return (principal.IsInRole("Editor") 
                && (PilotController.CurrentUserPilot.ClubId > 0) 
                && (PilotController.CurrentUserPilot.ClubId == ClubController.CurrentClub.ClubId));
        }
        
        /// <summary>
        /// Validates if the visiting request is bound to a pilot profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <example>in razor you can simply ask Request.IsPilot</example>
        public static bool IsPilot(this IPrincipal principal)
        {
            if (principal == null)
                return false;

            return (PilotController.CurrentUserPilot != null
                && PilotController.CurrentUserPilot.PilotId > 0);
        }

        /// <summary>
        /// Returns the active pilot profile bound to the authenticated user session
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Pilot Pilot(this IPrincipal principal)
        {
            if (principal == null)
                return null;

            return PilotController.CurrentUserPilot;
        }
    }
}
