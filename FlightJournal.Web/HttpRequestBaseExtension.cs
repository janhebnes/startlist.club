using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FlightJournal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Globalization;
using FlightJournal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Controllers;

namespace FlightJournal.Web
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
        /// Returns the active pilot profile bound to the authenticated user session
        /// </summary>
        /// <param name="request"></param>
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
        /// <returns></returns>
        public static Club Club(this HttpRequestBase request)
        {
            if (request == null)
                return null;

            return ClubController.CurrentClub;
        }

    }
}