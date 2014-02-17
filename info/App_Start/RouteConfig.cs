using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace info
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Legislation",
                url: "{id}",
                defaults: new { controller = "Home", action = "Bill" },
                constraints: new { id = @"(S[A-Z]|H[A-Z]).{0,2}[0-9]{1,4}" });

            routes.MapRoute(
                name: "House",
                url: "house",
                defaults: new { controller = "Home", action = "House" });

            routes.MapRoute(
                name: "Senate",
                url: "senate",
                defaults: new { controller = "Home", action = "Senate" });

            routes.MapRoute(
                name: "Search",
                url: "search",
                defaults: new { controller = "Home", action = "Search" });

            routes.MapRoute(
                name: "myAccount",
                url: "acct/{action}",
                defaults: new { controller = "Account", action = "Index" });

            routes.MapRoute(
                name: "Home",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" });

            //www.mylegis.com/home
            //www.mylegis.com/senate
            //www.mylegis.com/state
            //www.mylegis.com/SB123
            //www.mylegis.com/acct
            //www.mylegis.com/acct/logoff
            //www.mylegis.com/acct/logon

        }
    }
}