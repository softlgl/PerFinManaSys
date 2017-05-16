using PerFinManaSys.Web.Auth;
using System.Web.Mvc;

namespace PerFinManaSys.Web.Controllers
{
    [RequiresAuthentication]
    public class ErrorController : BaseController
    {
        /// <summary>
        /// 404页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Page404()
        {
            return View();
        }

        /// <summary>
        /// 500页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Page500()
        {
            return View();
        }
    }
}
