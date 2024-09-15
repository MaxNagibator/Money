using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ServiceWorker;

namespace MyMoneyWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Operation",
            //    url: "Operation",
            //    defaults: new { controller = "Home", action = "Operation", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Category",
            //    url: "Category",
            //    defaults: new { controller = "Home", action = "Category", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Debt",
            //    url: "Debt",
            //    defaults: new { controller = "Home", action = "Debt", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "About",
            //    url: "About",
            //    defaults: new { controller = "Home", action = "About", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Comment",
            //    url: "Comment",
            //    defaults: new { controller = "Home", action = "Comment", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Contact",
            //    url: "Contact",
            //    defaults: new { controller = "Home", action = "Contact", id = UrlParameter.Optional }
            //);
            routes.MapRoute(null, "DevGet/{service}/{method}/{*args}", new { controller = "Dev", action = "Get", id = UrlParameter.Optional});
            routes.MapRoute(null, "Dev/{service}/{method}/{*args}", new { controller = "Dev", action = "Post", id = UrlParameter.Optional});
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}