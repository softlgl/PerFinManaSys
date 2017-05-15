using PerFinManaSys.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Filter
{
    public class AppHandleErrorAttribute : HandleErrorAttribute
    {
         /// <summary>
        /// 全局异常处理
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            object obj = new object();
            lock (obj)
            {
                Exception ex = filterContext.Exception;
                LogHelper.Error(ex);
                filterContext.HttpContext.Server.ClearError();//处理完及时清理异常
                //转向
                filterContext.ExceptionHandled = true;
                int stateCode = (ex is HttpException) ? (ex is HttpException).GetHashCode() : 500;
                switch (stateCode)
                {
                    case 404:
                        filterContext.Result = new RedirectResult("/NotFound.html");
                        break;
                    default:
                        filterContext.Result = new RedirectResult("/Error.html");
                        break;
                }
            }
        }
    }
}