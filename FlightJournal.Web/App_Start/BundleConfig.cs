using System.Web.Optimization;

namespace FlightJournal.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.editorhookup.js", // extending the time format of the date picker
                        "~/Scripts/jquery.tablesorter.min.js"));    

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive-ajax.js",
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery.autocomplete-combobox.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/bootstrap-select.js"));



            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-select.css",
                      "~/Content/themes/base/core.css",
                      "~/Content/themes/base/autocomplete.css",
                      "~/Content/themes/base/datepicker.css",
                      "~/Content/themes/base/theme.css",
                      "~/Content/site.css"));
                      //"~/Content/themes/base/resizable.css",
                      //"~/Content/themes/base/selectable.css",
                      //"~/Content/themes/base/accordion.css",
                      //"~/Content/themes/base/button.css",
                      //"~/Content/themes/base/dialog.css",
                      //"~/Content/themes/base/slider.css",
                      //"~/Content/themes/base/tabs.css",                      
                      //"~/Content/themes/base/progressbar.css",
                      //"~/Content/themes/base/theme.css"
            
            //// When running release it is expected that the bundling automatically takes place
            //// BundleTable.EnableOptimizations = true;

        }
    }
}
