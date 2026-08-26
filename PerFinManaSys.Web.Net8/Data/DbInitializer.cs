using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PerFinManaSys.Web.Net8.Models;

namespace PerFinManaSys.Web.Net8.Data;

/// <summary>
/// 数据库初始化：插入系统默认数据（如系统级消费类别），并保证幂等、可重复执行。
/// </summary>
public static class DbInitializer
{
    /// <summary>系统级默认消费类别名称（C_UsersID=Guid.Empty 表示系统级）</summary>
    private static readonly string[] DefaultExpenseCategories =
    {
        "餐饮美食", "交通出行", "服饰美容", "居家生活",
        "休闲娱乐", "医疗健康", "学习进修", "人情往来", "其他支出"
    };

    /// <summary>
    /// 在已创建的根作用域中执行初始化。
    /// 失败时仅记录警告，不影响应用启动。
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MoneyWatcherDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DbInitializer");
        try
        {
            SeedSystemCategories(db);
            logger.LogInformation("数据库初始化完成。");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "数据库初始化失败（可忽略，手动在模块管理添加类别即可）。");
        }
    }

    /// <summary>
    /// 按名称去重增量插入系统默认消费类别。
    /// 使用 string 比较判断已存在项，避免 Guid.Empty 在 MySQL 下比较失效导致重复插入。
    /// </summary>
    private static void SeedSystemCategories(MoneyWatcherDbContext db)
    {
        // 已存在的系统类别名称集合（string 比较在 MySQL 下可靠）
        var existing = new HashSet<string>(
            db.Category
                .Where(c => DefaultExpenseCategories.Contains(c.C_Name))
                .Select(c => c.C_Name)
                .Where(n => n != null)
                .Select(n => n!));

        foreach (var name in DefaultExpenseCategories)
        {
            if (existing.Contains(name))
            {
                continue;
            }

            db.Category.Add(new Category
            {
                C_ID = Guid.NewGuid(),
                C_Name = name,
                C_Type = 0,               // 0=支出
                C_UsersID = Guid.Empty     // 系统默认
            });
        }

        db.SaveChanges();
    }
}
