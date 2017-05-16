using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PerFinManaSys.Web.Route
{
    public class BaseRoute
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //登陆页面路由
            routes.MapRoute(
               name: "LoginRoute",
               url: "Login.html",
               defaults: new { controller = "Login", action = "Index" }
           );
            var route = new { controller = "MainFrame", action = "Index" };
            //系统管理页面路由
            routes.MapRoute(
               name: "SystemRoute",
               url: "Main.html",
               defaults: route
           );
        }
    }
}