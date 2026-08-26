using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Models;
using PerFinManaSys.Web.Net8.Tools;

namespace PerFinManaSys.Web.Net8.Controllers;

/// <summary>
/// 登录日志管理（对齐前端 Views/Logs/Logins.cshtml 契约）
/// </summary>
[UserAuthorize]
public class LogsController : BaseController
{
    public LogsController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult Logins()
    {
        return View();
    }

    /// <summary>登录日志分页（EasyUI datagrid 加载 + 导出 Excel）</summary>
    public async Task<IActionResult> GetLoginsPaging(string begindate, string enddate, int page = 1, int rows = 10, string export = "")
    {
        var query = Db.Logins.AsNoTracking();

        if (DateTime.TryParse(begindate, out var begin) && DateTime.TryParse(enddate, out var end))
        {
            var endDay = end.Date.AddDays(1);
            query = query.Where(l => l.L_LoginTime >= begin.Date && l.L_LoginTime < endDay);
        }

        // left join Users 补齐用户名与邮箱（避免 L_UID 无匹配用户时日志被内连接过滤掉）
        var joined = query.GroupJoin(
            Db.Users.AsNoTracking(),
            l => l.L_UID,
            u => u.U_ID,
            (l, us) => new { l, us })
            .SelectMany(
                x => x.us.DefaultIfEmpty(),
                (x, u) => new
                {
                    x.l.L_ID,
                    U_Name = u != null ? u.U_Name : x.l.L_UserName,
                    Role = u != null ? u.U_Role : (x.l.L_Role),
                    U_Tel = u != null ? u.U_Tel : (string?)null,
                    x.l.L_HostName,
                    x.l.L_IP,
                    x.l.L_City,
                    x.l.L_LoginTime
                });

        // 导出：全量数据生成 Excel
        if (export == "exp")
        {
            var all = await joined.OrderByDescending(x => x.L_LoginTime).ToListAsync();
            var rowsData = all.Select(x => new object[]
            {
                x.U_Name ?? "",
                x.Role == (int)UserAuth.Admin ? "管理员" : "普通用户",
                x.U_Tel ?? "",
                x.L_HostName ?? "",
                x.L_IP ?? "",
                x.L_City ?? "",
                x.L_LoginTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();
            var bytes = ExportExcel.Export("登录日志", new[] { "用户名", "用户角色", "邮箱", "主机名称", "IP地址", "登录城市", "登录时间" }, rowsData);
            return File(bytes, "application/vnd.ms-excel", $"登录日志_{DateTime.Now:yyyyMMddHHmmss}.xls");
        }

        var total = await joined.CountAsync();
        var list = await joined
            .OrderByDescending(x => x.L_LoginTime)
            .Skip((page - 1) * rows)
            .Take(rows)
            .ToListAsync();

        var result = list.Select(x => new
        {
            x.L_ID,
            x.U_Name,
            Role = x.Role == (int)UserAuth.Admin ? "管理员" : "普通用户",
            x.U_Tel,
            x.L_HostName,
            x.L_IP,
            x.L_City,
            L_LoginTime = x.L_LoginTime.ToString("yyyy-MM-dd HH:mm:ss")
        });
        return MyJson(new { total, rows = result });
    }

    /// <summary>删除登录日志（支持批量，id 逗号分隔）</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteLogins(string id)
    {
        var ids = id.Split(',').Where(x => Guid.TryParse(x, out _)).Select(Guid.Parse).ToList();
        if (ids.Count == 0) return Json(new { flag = "False", Message = "请选择要删除的数据！" });
        var list = await Db.Logins.Where(l => ids.Contains(l.L_ID)).ToListAsync();
        Db.Logins.RemoveRange(list);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "删除成功！" });
    }
}
