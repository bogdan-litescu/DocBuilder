using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DocBuilder.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            // sites
            routes.MapRoute(
                name: "ShowPage",
                url: "docs/{ownerId}/{siteId}/{versionId}/{*pagePath}",
                defaults: new { 
                    controller = "Docs", 
                    action = "ShowPage",
                    pagePath = UrlParameter.Optional
                }
            );

        }
    }
}