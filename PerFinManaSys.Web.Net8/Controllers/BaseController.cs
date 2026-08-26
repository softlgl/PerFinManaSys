using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.MyActionResult;
using PerFinManaSys.Web.Net8.Tools;

namespace PerFinManaSys.Web.Net8.Controllers;

/// <summary>
/// 基类控制器（替代原 BaseController）。
/// 提供：Db 上下文、当前登录用户 user、MyJson 日期格式化输出、Entity 增删改辅助。
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly MoneyWatcherDbContext Db;
    protected readonly ICurrentUser Current;
    protected readonly LogHelper Log;

    protected BaseController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
    {
        Db = db;
        Current = current;
        Log = log;
    }

    /// <summary>当前登录用户（替代原 FormsAuth.GetUserData()）</summary>
    protected UserInfo? user => Current.User;

    /// <summary>日期格式化的 Json 结果（替代原 MyJson）</summary>
    protected CustomJsonResult MyJson(object data, string dateFormat = "yyyy-MM-dd")
    {
        return new CustomJsonResult { Data = data, DateFormat = dateFormat };
    }

    protected CustomJsonResult MyJson(object data, string dateFormat, JsonRequestBehavior behavior)
    {
        return new CustomJsonResult { Data = data, DateFormat = dateFormat, Behavior = behavior };
    }

    // ===== EntityState 辅助（替代 Db.Entry(x).State = ...） =====
    protected void AddEntity<T>(T entity) where T : class => Db.Entry(entity).State = EntityState.Added;
    protected void ModifyEntity<T>(T entity) where T : class => Db.Entry(entity).State = EntityState.Modified;
    protected void DeleteEntity<T>(T entity) where T : class => Db.Entry(entity).State = EntityState.Deleted;
}
