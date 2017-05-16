using System.Web.Mvc;
using System.Web.Security;

namespace PerFinManaSys.Web.Auth
{
    public class RequiresAuthentication : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.Url != null)
            {
                string returnUrl = filterContext.HttpContext.Request.Url.AbsolutePath;
                string redirectUrl = string.Format("?ReturnUrl={0}", returnUrl);
                string loginUrl = FormsAuthentication.LoginUrl + redirectUrl;
                if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) 
                    || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                    return;
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    filterContext.HttpContext.Response.Redirect(loginUrl, true);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Roles))
                    {
                        bool isAuthenticated = filterContext.HttpContext.User.IsInRole(Roles);
                        if (!isAuthenticated)
                        {
                            filterContext.HttpContext.Response.Redirect(loginUrl, true);
                        }
                    }
                }
            }
            //base.OnAuthorization(filterContext);
        }
    }
}