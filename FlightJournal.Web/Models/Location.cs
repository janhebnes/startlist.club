using System;
using System.Web.UI.WebControls;
using FlightJournal.Web.Translations;
using Microsoft.Ajax.Utilities;

namespace FlightJournal.Web.Models
{
    public class Location
    {
        public Location()
        {
            CreatedTimestamp = DateTime.Now;
            LastUpdatedTimestamp = DateTime.Now;
        }
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string ICAO { get; set; }
        [LocalizedDisplayName("Country")]
        public string Country { get; set; } // ISO Alpha-2 code based on https://www.iso.org/obp/ui/#search
        public DateTime CreatedTimestamp { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedTimestamp { get; set; }
        public string LastUpdatedBy { get; set; }
        public override string ToString()
        {
            return ToString(string.Empty);
        }
        /// <summary>
        /// Current country is used to change the formatting so local country is not shown .
        /// </summary>
        /// <param name="country">remember to convert to .ToUpperInvariant()</param>
        /// <returns></returns>
        public string ToString(string country)
        {
            // Shorten the string if current country
            if (!country.IsNullOrWhiteSpace() && country == Country && !ICAO.IsNullOrWhiteSpace())
                return this.Name + " " + ICAO + "";

            // Matching the format used by the OLC http://www.onlinecontest.org/ with ICAO instead of region
            if (!ICAO.IsNullOrWhiteSpace() && !Country.IsNullOrWhiteSpace())
                return this.Name + " " + ICAO + " (" + Country + ")";

            if (!Country.IsNullOrWhiteSpace())
                return this.Name + " (" + Country + ")";

            return Name;
        }
    }
}
