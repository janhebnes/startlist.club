using System;
using Microsoft.Ajax.Utilities;

namespace FlightJournal.Web.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string ICAO { get; set; }
        public string Country { get; set; } // ISO Alpha-2 code based on https://www.iso.org/obp/ui/#search
        public DateTime CreatedTimestamp { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedTimestamp { get; set; }
        public string LastUpdatedBy { get; set; }
        public override string ToString()
        {
            // Matching the format used by the OLC http://www.onlinecontest.org/ with ICAO instead of region
            if (!ICAO.IsNullOrWhiteSpace() && !Country.IsNullOrWhiteSpace())
                return this.Name + " (" + Country + " / " + ICAO + ")";

            return Name;
        }
    }
}
