using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJournal.Web.Models
{
    public class PilotSetEmailViewModel
    {
        public int PilotId { get; set; }
        public string Email { get; set; }
    }

    public class PilotSetMobilNumberViewModel
    {
        public int PilotId { get; set; }
        public string MobilNumber { get; set; }
    }
}
