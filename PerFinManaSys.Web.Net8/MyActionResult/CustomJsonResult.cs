using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace PerFinManaSys.Web.Net8.MyActionResult;

/// <summary>
/// 自定义 Json 结果（替代原 System.Web.Mvc.JsonResult + 日期格式化）。
/// 默认使用 Newtonsoft.Json 的日期格式，避免 JS 收到 /Date(...) / 形式。
/// </summary>
public class CustomJsonResult : ActionResult
{
    public object? Data { get; set; }
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public JsonRequestBehavior Behavior { get; set; } = JsonRequestBehavior.AllowGet;

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "application/json; charset=utf-8";

        var settings = new JsonSerializerSettings
        {
            DateFormatString = DateFormat,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None
        };
        var json = JsonConvert.SerializeObject(Data, settings);
        await response.WriteAsync(json);
    }
}

/// <summary>
/// 与原 System.Web.Mvc.JsonRequestBehavior 对齐的枚举（Core 中默认允许 GET）。
/// </summary>
public enum JsonRequestBehavior
{
    AllowGet = 0,
    DenyGet = 1
}
