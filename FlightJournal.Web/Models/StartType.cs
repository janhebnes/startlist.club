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

        public string LocalizedShortName
        {
            // BGA: Launch Type – W (Winch); B (Bungee); A (Aerotow); S (Self Launch); TMG (Touring Motor Glider)
            // Denmark: Start typer - S (Spilstart); F (Flyslæb); M (Selvstart)
            // Norge: Startmetodene - V (Vinsj); F (Flyslep); M (Selvstart);
            get
            {
                if (ShortName == "S" && Name == "Spilstart")
                    return ShortHand(_("W (Winch)"));
                else if (ShortName == "F" && Name == "Flyslæb")
                    return ShortHand(_("A (Aerotow)"));
                else if (ShortName == "M" && Name == "Selvstart")
                    return ShortHand(_("S (Self Launch)"));
                else
                    return ShortName;

                // Take the first short hand... 
                string ShortHand(string localizedDisplayName)
                {
                    return localizedDisplayName.Split(' ')[0];
                }
            }
        }

        [LocalizedDisplayName("Name")]
        public string Name { get; set; }

        public int? ClubId { get; set; }
        [LocalizedDisplayName("Club")]
        public virtual Club Club { get; set; }

        public string LocalizedDisplayName
        {
            // BGA: Launch Type – W (Winch); B (Bungee); A (Aerotow); S (Self Launch); TMG (Touring Motor Glider)
            // Denmark: Start typer - S (Spilstart); F (Flyslæb); M (Selvstart)
            // Norge: Startmetodene - V (Vinsj); F (Flyslep); M (Selvstart);
            get
            {
                if (ShortName == "S" && Name == "Spilstart")
                    return _("W (Winch)");
                else if (ShortName == "F" && Name == "Flyslæb")
                    return _("A (Aerotow)");
                else if (ShortName == "M" && Name == "Selvstart")
                    return _("S (Self Launch)");
                else
                    return Name;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        private string _(string resourceId)
        {
            return Internationalization.GetText(resourceId, Internationalization.LanguageCode);
        }
    }
}
