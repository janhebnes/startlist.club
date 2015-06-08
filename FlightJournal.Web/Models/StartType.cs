using System;
using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    [Serializable]
    public class StartType
    {
        [Key]
        public int StartTypeId { get; set; }
        [LocalizedDisplayName("Shortname")]
        public string ShortName { get; set; }
        [LocalizedDisplayName("Name")]
        public string Name { get; set; }

        public int? ClubId { get; set; }
        [LocalizedDisplayName("Club")]
        public virtual Club Club { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
