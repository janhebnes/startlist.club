using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Configuration
{
    /// <summary>
    /// wrapper for autoexport settings from web.config or Azure
    /// </summary>
    public class AutoExport
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TokenUrl { get; set; }
        public string PostUrl { get; set; }
        public int IntervalInMinutes { get; set; }
    }
}