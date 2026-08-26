using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Models;
using PerFinManaSys.Web.Net8.Tools;
using System.Security.Claims;

namespace PerFinManaSys.Web.Net8.Controllers;

public class LoginController : BaseController
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginController(
        MoneyWatcherDbContext db,
        ICurrentUser current,
        LogHelper log,
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor)
        : base(db, current, log)
    {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "MainFrame");
        }
        return View();
    }

    /// <summary>登录验证码图片</summary>
    public IActionResult GetCheckCode()
    {
        var code = ValidateCode.CreateValidateCode(4);
        HttpContext.Session.SetString(_config["AppSettings:SessionName"] ?? "CheckCode", code);
        var bytes = ValidateCode.CreateValidateGraphic(code);
        return File(bytes, "image/png");
    }

    [HttpPost]
    public async Task<IActionResult> CheckLogin(string username, string password, string checkCode, bool remeber = false)
    {
        try
        {
            var sessionCode = HttpContext.Session.GetString(_config["AppSettings:SessionName"] ?? "CheckCode");
            if (string.IsNullOrEmpty(sessionCode) || !string.Equals(sessionCode, checkCode, StringComparison.OrdinalIgnoreCase))
            {
                return MyJson(new { success = false, msg = "验证码错误！" });
            }

            var key = _config["AppSettings:DESKey"] ?? "PerFinManaSysKey";
            var encPwd = DES.Encrypt(password, key);

            var user = await Db.Users.FirstOrDefaultAsync(u => u.U_Name == username);
            if (user == null || user.U_PassWord != encPwd)
            {
                await WriteLoginLog(user, username, 1, "用户名或密码错误");
                return MyJson(new { success = false, msg = "用户名或密码错误！" });
            }
            if (user.U_Status != 0)
            {
                await WriteLoginLog(user, username, 1, "账号已被禁用");
                return MyJson(new { success = false, msg = "账号已被禁用！" });
            }

            // 更新最后登录时间
            user.U_LastLoginTime = DateTime.Now;
            Db.Users.Update(user);
            await Db.SaveChangesAsync();

            // 写登录日志
            await WriteLoginLog(user, username, 0, "登录成功");

            // 签发 Cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.U_Name ?? string.Empty),
                new Claim("U_Id", user.U_ID.ToString()),
                new Claim("U_Name", user.U_Name ?? string.Empty),
                new Claim("U_RealName", user.U_RealName ?? string.Empty),
                new Claim("U_Role", user.U_Role.ToString()),
                new Claim("U_IsAdmin", user.U_IsAdmin.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = remeber, ExpiresUtc = DateTimeOffset.Now.AddHours(2) });

            return MyJson(new { success = true, msg = "登录成功！" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "登录异常");
            return MyJson(new { success = false, msg = "登录失败：" + ex.Message });
        }
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("application/json") == true)
        {
            return Json(new { success = true });
        }
        return RedirectToAction("Index", "Login");
    }

    private async Task WriteLoginLog(Users? user, string userName, int result, string remark)
    {
        var ip = HttpHelperClientIp();
        var log = new Logins
        {
            L_ID = Guid.NewGuid(),
            L_UID = user?.U_ID ?? Guid.Empty,
            L_UserName = userName,
            L_HostName = Environment.MachineName,
            L_IP = ip,
            L_City = "本地",
            L_Role = user?.U_Role ?? 1,
            L_LoginTime = DateTime.Now,
            L_Result = result,
            L_Remark = remark
        };
        Db.Logins.Add(log);
        await Db.SaveChangesAsync();
    }

    private string HttpHelperClientIp()
    {
        var ctx = _httpContextAccessor.HttpContext;
        var ip = ctx?.Connection.RemoteIpAddress?.ToString();
        if (ctx?.Request.Headers.ContainsKey("X-Forwarded-For") ?? false)
        {
            ip = ctx.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
        }
        return ip ?? "unknown";
    }
}

