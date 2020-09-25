using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ImportB24
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
            name: "ParsTask",
            url: "parstask",
            defaults: new { controller = "Home", action = "ParsTask" }
            );
            routes.MapRoute(
            name: "Index",
            url: "index",
            defaults: new { controller = "Home", action = "Index" }
            );
            routes.MapRoute(
            name: "main",
            url: "",
            defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
            name: "CountParse",
            url: "countparse",
            defaults: new { controller = "Home", action = "CountParse" }
            );
            

        }
    }
}
