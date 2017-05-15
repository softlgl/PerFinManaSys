using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PerFinManaSys.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //登陆页面路由
            routes.MapRoute(
               name: "LoginRoute",
               url: "User/Login.html",
               defaults: new { controller = "Login", action = "Index"}
           );
            var route=new { controller = "MainFrame", action = "Index" };
            //系统管理页面路由
            routes.MapRoute(
               name: "SystemRoute",
               url: "User/Main.html",
               defaults: route
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "MainFrame", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}