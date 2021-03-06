﻿using System.Web;
using System.Web.Optimization;

namespace info
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            //Core frameworks
            bundles.Add(new ScriptBundle("~/bundles/frameworks").Include(
                                    "~/Scripts/jquery-{version}.js",
                                    "~/Scripts/jquery.validate.js",
                                    "~/Scripts/jquery.metadata.js",
                                    "~/Scripts/knockout-{version}.js",
                                    "~/Scripts/knockout.mapping.js",
                                    "~/Scripts/bootstrap.js",
                                    "~/Scripts/moment.js"));

            //Home/Bill - Knockout
            bundles.Add(new ScriptBundle("~/bundles/bill").Include(
                                    "~/Scripts/regex.js",
                                    "~/Scripts/jquery.dynatable.js",
                                    "~/Scripts/home.bill.js"));

            bundles.Add(new ScriptBundle("~/bundles/acct").Include(
                        "~/Scripts/regex.js",
                        "~/Scripts/jquery.dynatable.js",
                        "~/Scripts/home.acct.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                                     "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                                     "~/Content/bootstrap.css",
                                     "~/Content/bootstrap-theme.css",
                                     "~/Content/jquery.dynatable.css",
                                     "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/narrowjumbo").Include(
                                     "~/Content/jumbotron-narrow.css"));

            //bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            //            "~/Content/themes/base/jquery.ui.core.css",
            //            "~/Content/themes/base/jquery.ui.resizable.css",
            //            "~/Content/themes/base/jquery.ui.selectable.css",
            //            "~/Content/themes/base/jquery.ui.accordion.css",
            //            "~/Content/themes/base/jquery.ui.autocomplete.css",
            //            "~/Content/themes/base/jquery.ui.button.css",
            //            "~/Content/themes/base/jquery.ui.dialog.css",
            //            "~/Content/themes/base/jquery.ui.slider.css",
            //            "~/Content/themes/base/jquery.ui.tabs.css",
            //            "~/Content/themes/base/jquery.ui.datepicker.css",
            //            "~/Content/themes/base/jquery.ui.progressbar.css",
            //            "~/Content/themes/base/jquery.ui.theme.css"));

        }
    }
}