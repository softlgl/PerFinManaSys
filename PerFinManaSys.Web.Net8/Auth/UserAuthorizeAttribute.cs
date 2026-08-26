using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PerFinManaSys.Web.Net8.Auth;

/// <summary>
/// 登录授权过滤器（替代原 RequiresAuthentication / Request.IsAuthenticated 检查）。
/// 未登录跳转登录页；指定 Role=Admin 时要求管理员角色。
/// 方法级标注 [AllowAnonymous] 可放行（用于注册/找回密码等匿名接口）。
/// </summary>
public class UserAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>要求管理员角色</summary>
    public bool AdminOnly { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 方法级 [AllowAnonymous] 放行（注册/找回密码等匿名接口）
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>().Any();
        if (allowAnonymous) return;

        var user = context.HttpContext.User;
        if (user == null || !user.Identity!.IsAuthenticated)
        {
            if (context.HttpContext.Request.IsAjaxRequest())
            {
                context.Result = new JsonResult(new { success = false, msg = "未登录" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            else
            {
                context.Result = new RedirectToActionResult("Index", "Login", new { returnUrl = context.HttpContext.Request.Path });
            }
            return;
        }

        if (AdminOnly)
        {
            var roleClaim = user.FindFirst("U_Role")?.Value;
            var isAdmin = roleClaim == ((int)UserAuth.Admin).ToString();
            if (!isAdmin)
            {
                context.Result = new RedirectToActionResult("NoAuthorize", "Error", null);
            }
        }
    }
}

internal static class HttpRequestExtensions
{
    public static bool IsAjaxRequest(this HttpRequest request)
    {
        return request.Headers.XRequestedWith == "XMLHttpRequest"
            || (request.Headers.Referer.ToString().Contains("X-Requested-With", StringComparison.OrdinalIgnoreCase));
    }
}
