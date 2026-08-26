using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace PerFinManaSys.Web.Net8.Filters;

/// <summary>
/// 全局异常处理过滤器（替代原 AppHandleError）。
/// 捕获 Action 异常，记录日志并返回错误视图或 JSON。
/// 不会吞掉授权/状态码类异常（交由认证中间件或 UseExceptionHandler 处理）。
/// </summary>
public class AppHandleErrorAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<AppHandleErrorAttribute> _logger;

    public AppHandleErrorAttribute(ILogger<AppHandleErrorAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        // 记录日志，便于排查
        _logger.LogError(context.Exception, "全局异常捕获：{Path}", context.HttpContext.Request.Path);

        // 标记已处理，避免被 UseExceptionHandler 重复处理
        context.ExceptionHandled = true;

        // 判断是否 AJAX：兼容 X-Requested-With 头或 JSON 请求类型
        var isAjax = context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest" ||
                     context.HttpContext.Request.ContentType?.Contains("application/json") == true;

        if (isAjax)
        {
            context.Result = new JsonResult(new { success = false, msg = "服务器异常：" + context.Exception.Message })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        else
        {
            context.Result = new ViewResult
            {
                ViewName = "~/Views/Error/Index.cshtml",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                    new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                {
                    ["ErrorMessage"] = context.Exception.Message
                }
            };
        }
    }
}
