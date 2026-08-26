using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Models;
using PerFinManaSys.Web.Net8.Tools;

namespace PerFinManaSys.Web.Net8.Controllers;

/// <summary>
/// 收入管理（对齐前端 Views/Income/Index.cshtml 契约）
/// </summary>
[UserAuthorize]
public class IncomeController : BaseController
{
    public IncomeController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    /// <summary>收入列表（EasyUI datagrid 加载 + 导出 Excel）</summary>
    public async Task<IActionResult> GetIncomeData(string begindate, string enddate, int page = 1, int rows = 10, string export = "")
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Income.Where(i => i.I_UID == uid);

        if (DateTime.TryParse(begindate, out var begin) && DateTime.TryParse(enddate, out var end))
        {
            var endDay = end.Date.AddDays(1);
            query = query.Where(i => i.I_Date >= begin.Date && i.I_Date < endDay);
        }

        // 导出：全量数据生成 Excel
        if (export == "exp")
        {
            var all = await query.OrderByDescending(i => i.I_Date).ToListAsync();
            var rowsData = all.Select(i => new object[] { i.I_Amount, i.I_Date.ToString("yyyy-MM-dd"), i.I_Remark ?? "" }).ToList();
            var bytes = ExportExcel.Export("收入明细", new[] { "金额(元)", "进账日期", "备注" }, rowsData);
            return File(bytes, "application/vnd.ms-excel", $"收入明细_{DateTime.Now:yyyyMMddHHmmss}.xls");
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(i => i.I_Date)
            .Skip((page - 1) * rows)
            .Take(rows)
            .ToListAsync();

        var result = list.Select(i => new
        {
            i.I_ID,
            i.I_Amount,
            I_Date = i.I_Date.ToString("yyyy-MM-dd"),
            i.I_Remark
        });
        return MyJson(new { total, rows = result });
    }

    /// <summary>收入详情（编辑回填）</summary>
    [HttpPost]
    public async Task<IActionResult> GetIncomeDetail(Guid id)
    {
        var item = await Db.Income.FirstOrDefaultAsync(i => i.I_ID == id);
        if (item == null) return Json(new { flag = "False", Message = "记录不存在！" });
        return Json(new { item.I_ID, item.I_Amount, I_Date = item.I_Date.ToString("yyyy-MM-dd"), item.I_Remark, item.I_Title, item.I_Type });
    }

    /// <summary>新增收入</summary>
    [HttpPost]
    public async Task<IActionResult> AddIncome(Income model)
    {
        model.I_ID = Guid.NewGuid();
        model.I_UID = user?.U_ID ?? Guid.Empty;
        if (model.I_Date == default) model.I_Date = DateTime.Now;
        Db.Income.Add(model);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "添加成功！" });
    }

    /// <summary>修改收入</summary>
    [HttpPost]
    public async Task<IActionResult> EditIncome(Income model)
    {
        var entity = await Db.Income.FindAsync(model.I_ID);
        if (entity == null) return Json(new { flag = "False", Message = "记录不存在！" });
        entity.I_Amount = model.I_Amount;
        if (model.I_Date != default) entity.I_Date = model.I_Date;
        entity.I_Remark = model.I_Remark;
        entity.I_Title = model.I_Title;
        entity.I_Type = model.I_Type;
        Db.Income.Update(entity);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "修改成功！" });
    }

    /// <summary>删除收入（支持批量，id 逗号分隔）</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteIncome(string id)
    {
        var ids = id.Split(',').Where(x => Guid.TryParse(x, out _)).Select(Guid.Parse).ToList();
        if (ids.Count == 0) return Json(new { flag = "False", Message = "请选择要删除的数据！" });
        var list = await Db.Income.Where(i => ids.Contains(i.I_ID)).ToListAsync();
        Db.Income.RemoveRange(list);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "删除成功！" });
    }

    /// <summary>收入合计</summary>
    public async Task<IActionResult> GetIncomeSum(string begindate, string enddate)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Income.Where(i => i.I_UID == uid);
        if (DateTime.TryParse(begindate, out var begin) && DateTime.TryParse(enddate, out var end))
        {
            var endDay = end.Date.AddDays(1);
            query = query.Where(i => i.I_Date >= begin.Date && i.I_Date < endDay);
        }
        var sum = await query.SumAsync(i => (decimal?)i.I_Amount) ?? 0;
        return MyJson(new { sum });
    }
}
