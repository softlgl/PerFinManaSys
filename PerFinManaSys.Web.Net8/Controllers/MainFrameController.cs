using Microsoft.AspNetCore.Mvc;
using PerFinManaSys.Web.Net8.Data;

namespace PerFinManaSys.Web.Net8.Controllers;

[UserAuthorize]
public class MainFrameController : BaseController
{
    public MainFrameController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult Index()
    {
        ViewBag.UserName = user?.U_RealName ?? user?.U_Name ?? "游客";
        return View();
    }

    /// <summary>概览统计（首页卡片）</summary>
    [HttpPost]
    public async Task<IActionResult> GetOverview()
    {
            var uid = user?.U_ID ?? Guid.Empty;
        var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var incomeMonth = await Db.Income
            .Where(i => i.I_UID == uid && i.I_Date >= monthStart)
            .SumAsync(i => (decimal?)i.I_Amount) ?? 0;

        var expenseMonth = await Db.Expenses
            .Where(e => e.E_UID == uid && e.E_Date >= monthStart)
            .SumAsync(e => (decimal?)e.E_Amount) ?? 0;

        return MyJson(new
        {
            incomeMonth,
            expenseMonth,
            balance = incomeMonth - expenseMonth,
            userName = user?.U_RealName ?? user?.U_Name
        });
    }
}


