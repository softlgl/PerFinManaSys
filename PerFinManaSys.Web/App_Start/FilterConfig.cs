using PerFinManaSys.Web.Filter;
using System.Web;
using System.Web.Mvc;

namespace PerFinManaSys.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //全局异常filter注册
            filters.Add(new AppHandleErrorAttribute());
        }
    }
}