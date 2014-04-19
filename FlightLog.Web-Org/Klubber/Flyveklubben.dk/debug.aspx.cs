using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlightLog.Klubber.Flyveklubben.dk
{
    public partial class debug : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var xmlDate = "2011-10-02T00:00:00+02:00";

            var startDateString = xmlDate;
                    startDateString = startDateString.Replace("+02:00", "+00:00");
                    DateTime date = DateTime.Parse(startDateString);
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            
        }
    }
}