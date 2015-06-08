using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class PilotSetEmailViewModel
    {
        public int PilotId { get; set; }
        [LocalizedDisplayName("Email")]
        public string Email { get; set; }
    }

    public class PilotSetMobilNumberViewModel
    {
        public int PilotId { get; set; }
        [LocalizedDisplayName("Mobil number")]
        public string MobilNumber { get; set; }
    }
}
