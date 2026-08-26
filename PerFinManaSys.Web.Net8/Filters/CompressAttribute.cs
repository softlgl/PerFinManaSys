using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PerFinManaSys.Web.Net8.Filters;

/// <summary>
/// 响应压缩过滤器（替代原 CompressAttribute GZip）。
/// 仅在客户端支持 gzip/deflate 且非已压缩响应时开启。
/// 也可在 Program.cs 用 ResponseCompression 中间件统一处理。
/// </summary>
public class CompressAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var accept = context.HttpContext.Request.Headers.AcceptEncoding.ToString();
        if (accept.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            context.HttpContext.Response.Headers.ContentEncoding = "gzip";
            context.HttpContext.Response.Body = new System.IO.Compression.GZipStream(
                context.HttpContext.Response.Body, System.IO.Compression.CompressionMode.Compress);
        }
        else if (accept.Contains("deflate", StringComparison.OrdinalIgnoreCase))
        {
            context.HttpContext.Response.Headers.ContentEncoding = "deflate";
            context.HttpContext.Response.Body = new System.IO.Compression.DeflateStream(
                context.HttpContext.Response.Body, System.IO.Compression.CompressionMode.Compress);
        }
        base.OnActionExecuting(context);
    }
}
