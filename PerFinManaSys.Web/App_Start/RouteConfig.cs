using PerFinManaSys.Web.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PerFinManaSys.Web
{
    /// <summary>
    /// 页面路由
    /// </summary>
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            BaseRoute.RegisterRoutes(routes);
            ErrorRoute.RegisterRoutes(routes);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "MainFrame", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}