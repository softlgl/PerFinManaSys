using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Models;

namespace PerFinManaSys.Web.Net8.Controllers;

/// <summary>
/// 分类/模块管理（对齐前端 Views/Category/Index.cshtml 契约）
/// </summary>
[UserAuthorize]
public class CategoryController : BaseController
{
    public CategoryController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    /// <summary>分类分页（当前用户 + 系统默认）</summary>
    public async Task<IActionResult> GetCategoryPaging(int page = 1, int rows = 10)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Category.Where(c => c.C_UsersID == uid || c.C_UsersID == Guid.Empty);

        var total = await query.CountAsync();
        var list = await query
            .OrderBy(c => c.C_Name)
            .Skip((page - 1) * rows)
            .Take(rows)
            .ToListAsync();

        var result = list.Select(c => new
        {
            c.C_ID,
            c.C_UsersID,
            c.C_Name
        });
        return MyJson(new { total, rows = result });
    }

    /// <summary>分类下拉数据（EasyUI combobox，GET）。type：0=支出 1=收入，不传则返回全部</summary>
    public async Task<IActionResult> GetCategroyDrop(int? type = null)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var query = Db.Category
            .Where(c => c.C_UsersID == uid || c.C_UsersID == Guid.Empty);
        if (type.HasValue)
        {
            query = query.Where(c => c.C_Type == type.Value);
        }
        var list = await query
            .OrderBy(c => c.C_Name)
            .Select(c => new { c.C_ID, c.C_Name })
            .ToListAsync();
        return MyJson(list);
    }

    /// <summary>检测分类名称是否已存在（不存在返回 flag=True 可提交）</summary>
    [HttpPost]
    public async Task<IActionResult> IsExit(Category model)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        var exists = await Db.Category.AnyAsync(c =>
            c.C_Name == model.C_Name &&
            (c.C_UsersID == uid || c.C_UsersID == Guid.Empty) &&
            c.C_ID != model.C_ID);
        if (exists)
        {
            return Json(new { flag = "False", Message = "该模块名称已存在！" });
        }
        return Json(new { flag = "True", Message = "" });
    }

    /// <summary>新增分类</summary>
    [HttpPost]
    public async Task<IActionResult> AddCategory(Category model)
    {
        if (string.IsNullOrWhiteSpace(model.C_Name))
        {
            return Json(new { flag = "False", Message = "模块名称不能为空！" });
        }
        model.C_ID = Guid.NewGuid();
        model.C_UsersID = user?.U_ID ?? Guid.Empty;
        Db.Category.Add(model);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "添加成功！" });
    }

    /// <summary>修改分类</summary>
    [HttpPost]
    public async Task<IActionResult> EditCategory(Category model)
    {
        var entity = await Db.Category.FindAsync(model.C_ID);
        if (entity == null) return Json(new { flag = "False", Message = "记录不存在！" });
        entity.C_Name = model.C_Name;
        Db.Category.Update(entity);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "修改成功！" });
    }

    /// <summary>删除分类（支持批量，id 逗号分隔）</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var ids = id.Split(',').Where(x => Guid.TryParse(x, out _)).Select(Guid.Parse).ToList();
        if (ids.Count == 0) return Json(new { flag = "False", Message = "请选择要删除的数据！" });
        var list = await Db.Category.Where(c => ids.Contains(c.C_ID)).ToListAsync();
        Db.Category.RemoveRange(list);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "删除成功！" });
    }
}
