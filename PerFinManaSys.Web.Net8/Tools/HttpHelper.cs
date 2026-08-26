using Microsoft.AspNetCore.Http;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// HTTP 辅助（替代原 HttpContext.Current.Request 用法）。
/// 通过注入 IHttpContextAccessor 获取当前请求。
/// </summary>
public class HttpHelper
{
    private readonly IHttpContextAccessor _accessor;

    public HttpHelper(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string ClientIp
    {
        get
        {
            var ctx = _accessor.HttpContext;
            if (ctx == null) return "unknown";
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            // 反向代理场景可读取 X-Forwarded-For
            if (ctx.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = ctx.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
            }
            return ip ?? "unknown";
        }
    }
}
