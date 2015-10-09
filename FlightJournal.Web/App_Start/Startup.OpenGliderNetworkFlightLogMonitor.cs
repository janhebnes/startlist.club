using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlightJournal.Web.Models;
using Owin;

namespace FlightJournal.Web
{
    public partial class Startup
    {
        public void ConfigureOpenGliderNetworkFlightLogMonitor(IAppBuilder app)
        {
            // TODO: Do we create a new Db Context for Ogn log data .. ?<s
            //app.CreatePerOwinContext(ApplicationDbContext.Create);

            //http://live.glidernet.org/flightlog/index.php?a=EHDL&s=QFE&u=M&z=2&p=&d=30052015&j
            //http://stackoverflow.com/questions/32401704/appending-json-data-to-listview-c-sharp

    //        WebClient client = new WebClient();
    //        string json = client.DownloadString("http://live.glidernet.org/flightlog/index.php?a=EHDL&s=QFE&u=M&z=2&p=&d=30052015&j");

    //        JObject data = JObject.Parse(json);

    //        // create an array of ListViewItems from the JSON
    //        var items = data["flights"]
    //            .Children<JObject>()
    //            .Select(jo => new ListViewItem(new string[] 
    //{
    //    (string)jo["glider"],
    //    (string)jo["takeoff"],
    //    (string)jo["glider_landing"],
    //    (string)jo["glider_time"]
    //}))
    //            .ToArray();

            // NOTE: Should we owin the FlightContext  see the use of context.Get<ApplicationDbContext>() that allows for a single instance to be used all over... 
        }
    }
}