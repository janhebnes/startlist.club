using System.Web.Optimization;

namespace FlightJournal.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/intro").Include(
                        "~/Scripts/introjs/intro.min.js"));

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

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-select.js"));

            // Core DataTables + required extensions (only those we actively use)
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                      "~/Scripts/DataTables/jquery.dataTables.min.js",
                      "~/Scripts/DataTables/dataTables.bootstrap.min.js",
                      // Core feature extensions
                      "~/Scripts/DataTables/dataTables.responsive.min.js",
                      "~/Scripts/DataTables/dataTables.buttons.min.js",
                      "~/Scripts/DataTables/dataTables.colReorder.min.js",
                      "~/Scripts/DataTables/dataTables.fixedHeader.min.js",
                      "~/Scripts/DataTables/dataTables.keyTable.min.js",
                      // Additional requested extensions
                      "~/Scripts/DataTables/dataTables.rowGroup.min.js",
                      "~/Scripts/DataTables/dataTables.rowReorder.min.js",
                      "~/Scripts/DataTables/dataTables.scroller.min.js",
                      "~/Scripts/DataTables/dataTables.select.min.js",
                      "~/Scripts/DataTables/dataTables.autoFill.min.js",
                      "~/Scripts/DataTables/dataTables.fixedColumns.min.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/charting").Include(
                      "~/Scripts/moment-with-locales.min.js",
                      "~/Scripts/chart.min.js",
                      "~/Scripts/chartjs-plugin-crosshair.js",
                      "~/Scripts/Charting.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-select.css",
                      "~/Content/themes/base/core.css",
                      "~/Content/themes/base/autocomplete.css",
                      "~/Content/themes/base/datepicker.css",
                      "~/Content/themes/base/theme.css",
                      "~/Content/site.css",
                      "~/Content/introjs/introjs.min.css",
                      // DataTables styling (core + extensions used)
                      "~/Content/DataTables/css/jquery.dataTables.min.css",
                      "~/Content/DataTables/css/dataTables.bootstrap.min.css",
                      "~/Content/DataTables/css/responsive.dataTables.min.css",
                      "~/Content/DataTables/css/buttons.dataTables.min.css",
                      "~/Content/DataTables/css/colReorder.dataTables.min.css",
                      "~/Content/DataTables/css/fixedHeader.dataTables.min.css",
                      "~/Content/DataTables/css/keyTable.dataTables.min.css",
                      // Additional requested extensions
                      "~/Content/DataTables/css/rowGroup.dataTables.min.css",
                      "~/Content/DataTables/css/rowReorder.dataTables.min.css",
                      "~/Content/DataTables/css/scroller.dataTables.min.css",
                      "~/Content/DataTables/css/select.dataTables.min.css",
                      "~/Content/DataTables/css/autoFill.dataTables.min.css",
                      "~/Content/DataTables/css/fixedColumns.dataTables.min.css"
                      ));

#if CFG_RELEASE
            // Enable minification + bundling in release
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
