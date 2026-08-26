using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Models;
using PerFinManaSys.Web.Net8.Tools;

namespace PerFinManaSys.Web.Net8.Controllers;

[UserAuthorize(AdminOnly = true)]
public class UserManageController : BaseController
{
    public UserManageController(MoneyWatcherDbContext db, ICurrentUser current, LogHelper log)
        : base(db, current, log)
    {
    }

    public IActionResult UserList()
    {
        var uid = user?.U_ID ?? Guid.Empty;
        return View();
    }

    public IActionResult Add()
    {
        var uid = user?.U_ID ?? Guid.Empty;
        return View();
    }

    /// <summary>用户列表数据（EasyUI DataGrid）</summary>
    [HttpPost]
    public async Task<IActionResult> UserListData(int page = 1, int rows = 10)
    {
        var query = Db.Users.AsQueryable();
        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(u => u.U_RegistDate)
            .Skip((page - 1) * rows)
            .Take(rows)
            .ToListAsync();

        var result = list.Select(u => new
        {
            u.U_ID,
            u.U_Name,
            U_PassWord = Tools.DES.Decrypt(u.U_PassWord ?? "", "PerFinManaSysKey"),
            u.U_Tel,
            u.U_Email,
            u.U_RealName,
            u.U_Role,
            U_RegistDate = u.U_RegistDate.ToString("yyyy-MM-dd"),
            U_LastLoginTime = u.U_LastLoginTime?.ToString("yyyy-MM-dd HH:mm:ss"),
            u.U_Status
        });
        return MyJson(new { total, rows = result });
    }

    /// <summary>注册（匿名，前台调用）</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> AddUser(Users _user)
    {
        _user.U_Role = (int)UserAuth.User;
        _user.U_IsAdmin = false;
        return await InsertUser(_user);
    }

    /// <summary>管理端新增用户</summary>
    [HttpPost]
    public async Task<IActionResult> ManageAddUser(Users _user)
    {
        return await InsertUser(_user);
    }

    private async Task<IActionResult> InsertUser(Users _user)
    {
        if (string.IsNullOrEmpty(_user.U_Name) || string.IsNullOrEmpty(_user.U_PassWord) || string.IsNullOrEmpty(_user.U_Tel))
        {
            return Json(new { flag = "False", Message = "请填写完整登录信息！" });
        }
        Regex r = new Regex(@"^\s*([A-Za-z0-9_-]+(\.\w+)*@(\w+\.)+\w{2,5})\s*$");
        if (!r.IsMatch(_user.U_Tel))
        {
            return Json(new { flag = "False", Message = "邮箱格式不正确！" });
        }
        var exist = await Db.Users.FirstOrDefaultAsync(t => t.U_Name == _user.U_Name);
        if (exist != null)
        {
            return Json(new { flag = "Exits", Message = "该用户名已被注册！" });
        }

        _user.U_ID = Guid.NewGuid();
        _user.U_PassWord = Tools.DES.Encrypt(_user.U_PassWord, "PerFinManaSysKey");
        _user.U_RegistDate = DateTime.Now;
        _user.U_LastLoginTime = null;
        Db.Users.Add(_user);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "注册成功！" });
    }

    /// <summary>修改用户</summary>
    [HttpPost]
    public async Task<IActionResult> UpdateUser(Users _user)
    {
        if (string.IsNullOrEmpty(_user.U_Name) || string.IsNullOrEmpty(_user.U_Tel))
        {
            return Json(new { flag = "False", Message = "请填写完整登录信息！" });
        }
        var entity = await Db.Users.FindAsync(_user.U_ID);
        if (entity == null)
        {
            return Json(new { flag = "False", Message = "该用户不存在！" });
        }
        entity.U_Name = _user.U_Name;
        entity.U_RealName = _user.U_RealName;
        entity.U_Tel = _user.U_Tel;
        entity.U_Email = _user.U_Email;
        entity.U_Role = _user.U_Role;
        entity.U_IsAdmin = _user.U_Role == (int)UserAuth.Admin;
        if (!string.IsNullOrEmpty(_user.U_PassWord))
        {
            entity.U_PassWord = Tools.DES.Encrypt(_user.U_PassWord, "PerFinManaSysKey");
        }
        Db.Users.Update(entity);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "修改成功！" });
    }

    /// <summary>判断用户名是否存在（前台注册用）</summary>
    [HttpPost]
    public async Task<IActionResult> IsExit(Users _user)
    {
        var exist = await Db.Users.FirstOrDefaultAsync(t => t.U_Name == _user.U_Name);
        if (exist != null)
        {
            return Json(new { flag = "False", Message = "该用户名已被注册！" });
        }
        return Json(new { flag = "True", Message = "" });
    }

    /// <summary>删除用户（支持批量，id 逗号分隔）</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { flag = "False", Message = "请选择要删除的数据！" });
        }
        var ids = id.Split(',').Where(x => Guid.TryParse(x, out _)).Select(Guid.Parse).ToList();
        if (ids.Count == 0)
        {
            return Json(new { flag = "False", Message = "请选择要删除的数据！" });
        }
        var users = await Db.Users.Where(u => ids.Contains(u.U_ID)).ToListAsync();
        if (users.Count == 0)
        {
            return Json(new { flag = "False", Message = "该用户不存在！" });
        }
        Db.Users.RemoveRange(users);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "删除成功！" });
    }

    /// <summary>重置密码</summary>
    [HttpPost]
    public async Task<IActionResult> ResetPwd(Guid id)
    {
        var entity = await Db.Users.FindAsync(id);
        if (entity == null) return Json(new { flag = "False", Message = "用户不存在" });
        entity.U_PassWord = Tools.DES.Encrypt("123456", "PerFinManaSysKey");
        Db.Users.Update(entity);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "密码已重置为 123456！" });
    }

    /// <summary>找回密码：校验用户名 + 邮箱（匿名）</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> FindUserInfo(Users _user)
    {
        if (string.IsNullOrEmpty(_user.U_Name) || string.IsNullOrEmpty(_user.U_Tel))
        {
            return Json(new { flag = "False", Message = "请填写用户名和邮箱！" });
        }
        var entity = await Db.Users.FirstOrDefaultAsync(t => t.U_Name == _user.U_Name && t.U_Tel == _user.U_Tel);
        if (entity == null)
        {
            return Json(new { flag = "False", Message = "用户名与邮箱不匹配！" });
        }
        HttpContext.Session.SetString("ResetUserId", entity.U_ID.ToString());
        return Json(new { flag = "Success", Message = "验证通过！" });
    }

    /// <summary>找回密码：设置新密码（匿名）</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> UpUserPass(string passWord)
    {
        var uidStr = HttpContext.Session.GetString("ResetUserId");
        if (string.IsNullOrEmpty(uidStr) || !Guid.TryParse(uidStr, out var uid))
        {
            return Json(new { flag = "False", Message = "请先进行身份验证！" });
        }
        var entity = await Db.Users.FindAsync(uid);
        if (entity == null)
        {
            return Json(new { flag = "False", Message = "用户不存在！" });
        }
        entity.U_PassWord = Tools.DES.Encrypt(passWord, "PerFinManaSysKey");
        Db.Users.Update(entity);
        await Db.SaveChangesAsync();
        HttpContext.Session.Remove("ResetUserId");
        return Json(new { flag = "Success", Message = "密码修改成功！" });
    }

    /// <summary>登录后修改密码（表单字段 U_PassWord / U_PassWordConfirm）</summary>
    [HttpPost]
    public async Task<IActionResult> UpdatePass(string U_PassWord, string U_PassWordConfirm)
    {
        var uid = user?.U_ID ?? Guid.Empty;
        if (uid == Guid.Empty)
        {
            return Json(new { flag = "False", Message = "请先登录！" });
        }
        if (string.IsNullOrEmpty(U_PassWord) || string.IsNullOrEmpty(U_PassWordConfirm))
        {
            return Json(new { flag = "False", Message = "请输入完整信息！" });
        }
        var entity = await Db.Users.FindAsync(uid);
        if (entity == null)
        {
            return Json(new { flag = "False", Message = "用户不存在！" });
        }
        if (entity.U_PassWord != Tools.DES.Encrypt(U_PassWord, "PerFinManaSysKey"))
        {
            return Json(new { flag = "False", Message = "原密码错误！" });
        }
        entity.U_PassWord = Tools.DES.Encrypt(U_PassWordConfirm, "PerFinManaSysKey");
        Db.Users.Update(entity);
        await Db.SaveChangesAsync();
        return Json(new { flag = "Success", Message = "密码修改成功！" });
    }
}
