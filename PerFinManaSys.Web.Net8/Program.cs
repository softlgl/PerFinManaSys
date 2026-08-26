using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Auth;
using PerFinManaSys.Web.Net8.Data;
using PerFinManaSys.Web.Net8.Filters;
using Microsoft.Extensions.Logging;

namespace PerFinManaSys.Web.Net8;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ===== 配置 =====
        var configuration = builder.Configuration;

        // 日志：log4net（保留内置 Console 提供程序，控制台与文件同时输出）
        builder.Logging.AddLog4Net("log4net.config");

        // ===== 服务 =====
        builder.Services.AddControllersWithViews()
            .AddNewtonsoftJson(options =>
            {
                // 兼容旧版日期格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

        // EF Core + MySQL (Pomelo) — Code First
        builder.Services.AddDbContext<MoneyWatcherDbContext>(options =>
            options.UseMySql(
            configuration.GetConnectionString("MoneyWatcherDB"),
            new MySqlServerVersion(new Version(8, 0, 36)),
            mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            }));

        // Session（验证码使用）
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(20);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Cookie 认证
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login/Index";
                options.AccessDeniedPath = "/Error/NoAuthorize";
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "PerFinAuth";
            });

        // 当前用户访问器
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();
        builder.Services.AddScoped<LogHelper>();
        builder.Services.AddScoped<HttpHelper>();

        // 全局异常过滤器
        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add<AppHandleErrorAttribute>();
        });

        var app = builder.Build();

        // ===== 中间件管道 =====
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error/Index");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseSession();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // 初始化系统默认数据（数据库类别等，幂等、可重复执行）
        DbInitializer.Initialize(app.Services);

        // 传统路由（兼容 Login.html 等旧 URL 通过 Rewrite，这里保留默认 MVC 路由）
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=MainFrame}/{action=Index}/{id?}");

        app.Run();
    }
}
