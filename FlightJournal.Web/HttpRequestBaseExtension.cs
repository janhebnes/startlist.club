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

            return (Pilot.GetCurrentUserPilot().PilotId > 0);
        }

    }
}