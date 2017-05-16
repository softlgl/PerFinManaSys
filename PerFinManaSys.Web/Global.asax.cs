using Newtonsoft.Json;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Enum;
using PerFinManaSys.Web.Models;
using PerFinManaSys.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace PerFinManaSys.Web
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //隐藏mvc版本标头出现
            MvcHandler.DisableMvcResponseHeader = true;

            //初始化log4net
            LogHelper.LogConfig(Server.MapPath(@"~\App_Data\log4net.config"));
        }

        protected void Application_AuthenticateRequest(Object sender,EventArgs e)
        {
            Users user = FormsAuth.GetUserData();
            string role = user.U_Role == 0 ? UserAuth.Admin : UserAuth.User;
            string[] roles = { role };
            if (Context.User != null)
            {
                Context.User = new System.Security.Principal.GenericPrincipal(Context.User.Identity, roles);
            }

            //Users userinfo = FormsAuth.GetUserData();
            //string uid=userinfo.U_ID==Guid.Empty?null:userinfo.U_ID.ToString();
            //UserInfoPrincipal newUser = new UserInfoPrincipal(uid, userinfo.U_Name, userinfo.U_Role);
            //HttpContext.Current.User = newUser;
        }

    }
}