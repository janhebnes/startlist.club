using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class Manouvre
    {
        [Key]
        public int ManouvreId { get; set; }

        [LocalizedDisplayName("Name")]
        [AllowHtml]
        public string ManouvreItem { get; set; }
        [LocalizedDisplayName("Description")]
        [AllowHtml]
        public string Description { get; set; }
        [LocalizedDisplayName("FontAwesome CSS class to use for icon")]
        [AllowHtml]
        public string IconCssClass { get; set; }
        public int Icon { get; set; }
        public virtual ManouvreIcon ManouvreIcon {get; set;}
    }
}