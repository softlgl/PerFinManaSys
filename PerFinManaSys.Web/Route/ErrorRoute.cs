using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PerFinManaSys.Web.Route
{
    /// <summary>
    /// 异常页面路由
    /// </summary>
    public class ErrorRoute
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //404页面配置
            routes.MapRoute(
               name: "Error404",
               url: "404",
               defaults: new { controller = "Error", action = "Page404", }
           );
            //500页面配置
            routes.MapRoute(
               name: "Error500",
               url: "500",
               defaults: new { controller = "Error", action = "Page500", }
           );
        }
    }
}