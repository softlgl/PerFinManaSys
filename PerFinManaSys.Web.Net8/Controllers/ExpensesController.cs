using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Models;
using PerFinManaSys.Web.Net8.Tools;

namespace PerFinManaSys.Web.Net8.Controllers;

/// <summary>
/// 支出/消费管理（对齐前端 Views/Expenses/Index.cshtml 契约）
/// </summary>
[UserAuthorize]
public class ExpensesController : BaseController
{
    public ExpensesController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    /// <summary>消费列表（EasyUI datagrid 加载 + 导出 Excel）</summary>
    public async Task<IActionResult> GetExpenseData(string Categroy, string begindate, string enddate, int page = 1, int rows = 10, string export = "")
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Expenses.Where(e => e.E_UID == uid);

        if (Guid.TryParse(Categroy, out var cid) && cid != Guid.Empty)
        {
            query = query.Where(e => e.E_Type == cid);
        }
        if (DateTime.TryParse(begindate, out var begin) && DateTime.TryParse(enddate, out var end))
        {
            var endDay = end.Date.AddDays(1);
            query = query.Where(e => e.E_Date >= begin.Date && e.E_Date < endDay);
        }

        // 分类名称字典（用于列表展示）
        var categories = await Db.Category.ToDictionaryAsync(c => c.C_ID, c => c.C_Name ?? "");

        // 导出：全量数据生成 Excel
        if (export == "exp")
        {
            var all = await query.OrderByDescending(e => e.E_Date).ToListAsync();
            var rowsData = all.Select(e => new object[]
            {
                categories.TryGetValue(e.E_Type, out var n) ? n : "",
                e.E_Amount,
                e.E_Date.ToString("yyyy-MM-dd"),
                e.E_Remark ?? ""
            }).ToList();
            var bytes = ExportExcel.Export("消费明细", new[] { "消费类型", "金额(元)", "日期", "备注" }, rowsData);
            return File(bytes, "application/vnd.ms-excel", $"消费明细_{DateTime.Now:yyyyMMddHHmmss}.xls");
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(e => e.E_Date)
            .Skip((page - 1) * rows)
            .Take(rows)
            .ToListAsync();

        var result = list.Select(e => new
        {
            e.E_ID,
            E_Type = categories.TryGetValue(e.E_Type, out var n) ? n : "",
            e.E_Amount,
            E_Date = e.E_Date.ToString("yyyy-MM-dd"),
            e.E_Remark
        });
        return MyJson(new { total, rows = result });
    }

    /// <summary>消费详情（编辑回填，E_Type 返回 Guid 供 combobox 选中）</summary>
    [HttpPost]
    public async Task<IActionResult> ExpenseDetail(Guid id)
    {
        var item = await Db.Expenses.FirstOrDefaultAsync(e => e.E_ID == id);
        if (item == null) return Json(new { flag = "False", Message = "记录不存在！" });
        return Json(new { item.E_ID, item.E_Type, item.E_Amount, E_Date = item.E_Date.ToString("yyyy-MM-dd"), item.E_Remark });
    }

    /// <summary>新增消费</summary>
    [HttpPost]
    public async Task<IActionResult> AddExpenses(Expenses model)
    {
        model.E_ID = Guid.NewGuid();
        model.E_UID = user?.U_ID ?? Guid.Empty;
        if (model.E_Date == default) model.E_Date = DateTime.Now;
        Db.Expenses.Add(model);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "添加成功！" });
    }

    /// <summary>修改消费</summary>
    [HttpPost]
    public async Task<IActionResult> EditExpenses(Expenses model)
    {
        var entity = await Db.Expenses.FindAsync(model.E_ID);
        if (entity == null) return Json(new { flag = "False", Message = "记录不存在！" });
        entity.E_Type = model.E_Type;
        entity.E_Amount = model.E_Amount;
        entity.E_Date = model.E_Date == default ? DateTime.Now : model.E_Date;
        entity.E_Remark = model.E_Remark;
        Db.Expenses.Update(entity);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "修改成功！" });
    }

    /// <summary>删除消费（支持批量，id 逗号分隔）</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteExpenses(string id)
    {
        var ids = id.Split(',').Where(x => Guid.TryParse(x, out _)).Select(Guid.Parse).ToList();
        if (ids.Count == 0) return Json(new { flag = "False", Message = "请选择要删除的数据！" });
        var list = await Db.Expenses.Where(e => ids.Contains(e.E_ID)).ToListAsync();
        Db.Expenses.RemoveRange(list);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "删除成功！" });
    }

    /// <summary>消费总额与日平均（Categroy/begindate/enddate 可选）</summary>
    public async Task<IActionResult> GetSumAvg(string Categroy, string begindate, string enddate)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Expenses.Where(e => e.E_UID == uid);

        if (Guid.TryParse(Categroy, out var cid) && cid != Guid.Empty)
        {
            query = query.Where(e => e.E_Type == cid);
        }
        DateTime? begin = null, end = null;
        if (DateTime.TryParse(begindate, out var b)) begin = b;
        if (DateTime.TryParse(enddate, out var e2)) end = e2;
        if (begin.HasValue && end.HasValue)
        {
            var endDay = end.Value.Date.AddDays(1);
            query = query.Where(x => x.E_Date >= begin.Value.Date && x.E_Date < endDay);
        }

        var total = await query.SumAsync(e => (decimal?)e.E_Amount) ?? 0;
        int days = 1;
        if (begin.HasValue && end.HasValue)
        {
            days = (end.Value.Date - begin.Value.Date).Days + 1;
            if (days < 1) days = 1;
        }
        var avg = Math.Round(total / days, 2);
        return MyJson(new { Total = total, Avg = avg });
    }
}
