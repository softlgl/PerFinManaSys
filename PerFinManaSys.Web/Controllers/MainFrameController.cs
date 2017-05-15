using PerFinManaSys.Web.Enum;
using PerFinManaSys.Web.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PerFinManaSys.Web.Auth;
using PerFinManaSys.Web.Models;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication]
    public class MainFrameController : Controller
    {
        /// <summary>
        /// 主面板
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = "个人财务管理系统";
            ViewBag.UserName = FormsAuth.GetUserData().U_Name;
            Users user = FormsAuth.GetUserData();
            return View();
        }

    }
}
