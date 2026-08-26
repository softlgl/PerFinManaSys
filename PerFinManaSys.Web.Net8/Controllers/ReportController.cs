using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Tools;

namespace PerFinManaSys.Web.Net8.Controllers;

/// <summary>
/// 统计报表（对齐前端 Views/Report/*.cshtml 契约）
/// </summary>
[UserAuthorize]
public class ReportController : BaseController
{
    public ReportController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult HalfYear()
    {
        return View();
    }

    public IActionResult ConsumptionScale()
    {
        return View();
    }

    /// <summary>近一年消费统计（柱状图：xdata 月份 / series 金额）</summary>
    public async Task<IActionResult> HalfYearData()
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var today = DateTime.Today;
        // 近 12 个月
        var start = new DateTime(today.Year, today.Month, 1).AddMonths(-11);

        var items = await Db.Expenses
            .AsNoTracking()
            .Where(e => e.E_UID == uid && e.E_Date >= start)
            .Select(e => new { e.E_Date, e.E_Amount })
            .ToListAsync();

        var xdata = new List<string>();
        var series = new List<decimal>();
        for (int i = 0; i < 12; i++)
        {
            var month = start.AddMonths(i);
            var key = $"{month.Year}-{month.Month}";
            xdata.Add($"{month.Year}-{month.Month:D2}");
            var sum = items.Where(x => x.E_Date.Year == month.Year && x.E_Date.Month == month.Month).Sum(x => x.E_Amount);
            series.Add(Math.Round(sum, 2));
        }
        return MyJson(new { xdata, series });
    }

    /// <summary>消费比例（饼图：xdata 分类名数组字符串 / series {value,name}）</summary>
    public async Task<IActionResult> ConsumptionData(string beginDate, string endDate)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Expenses.AsNoTracking().Where(e => e.E_UID == uid);

        if (DateTime.TryParse(beginDate, out var begin) && DateTime.TryParse(endDate, out var end))
        {
            var endDay = end.Date.AddDays(1);
            query = query.Where(e => e.E_Date >= begin.Date && e.E_Date < endDay);
        }

        // 按分类聚合
        var groups = await query
            .GroupBy(e => e.E_Type)
            .Select(g => new { E_Type = g.Key, Amount = g.Sum(x => x.E_Amount) })
            .ToListAsync();

        var cats = await Db.Category.AsNoTracking().ToDictionaryAsync(c => c.C_ID, c => c.C_Name ?? "未分类");

        var series = groups.Select(g => new
        {
            name = cats.TryGetValue(g.E_Type, out var n) ? n : "未分类",
            value = Math.Round(g.Amount, 2)
        }).ToList();

        // 前端 eval(data.xdata)，需为字符串数组形式
        var xdata = JsonConvert.SerializeObject(series.Select(s => s.name).ToArray());
        return MyJson(new { xdata, series });
    }
}
