# PerFinManaSys 个人财务管理系统

一个基于 ASP.NET 的个人财务/记账管理系统，支持收入与支出的录入、分类、统计与可视化。

## 项目结构

| 目录 | 说明 |
| --- | --- |
| `PerFinManaSys.Web` | 旧版本：VS2013 + MVC4 + SQL Server 2012（仅作历史保留，不再维护） |
| `PerFinManaSys.Web.Net8` | 新版本：.NET 8 + MySQL 8 + EF Core Code First（当前活跃版本） |

## 功能模块

- **登录**：验证码登录，密码 DES 加密，Cookie 认证（滑动过期 2 小时）
- **主框架**：首页概览卡片（当月收入 / 支出 / 结余）
- **收入管理**：录入、编辑、删除、查询收入记录
- **支出管理**：录入、编辑、删除、查询支出记录
- **分类管理**：自定义收支分类（按用户隔离）
- **用户管理**：用户、角色、管理员标识管理
- **登录日志**：记录登录历史
- **报表中心**：半年收支趋势图（基于 ECharts）
- **Excel 导出**：基于 NPOI 导出收支数据

## 技术栈（.NET 8 版本）

- **运行时**：.NET 8 / ASP.NET Core MVC
- **数据库**：MySQL 8.x + Pomelo.EntityFrameworkCore.MySql 8.0.2（EF Core Code First）
- **认证**：Cookie 认证 + 自定义 `UserAuthorizeAttribute`
- **日志**：log4net（控制台 + 文件）
- **Excel**：NPOI 2.7.1
- **前端**：jQuery + EasyUI + ECharts + layer
- **其他**：Newtonsoft.Json、System.Drawing.Common（验证码）

## 快速开始

### 1. 准备 MySQL
安装 MySQL 8.x，并执行 `PerFinManaSys.Web.Net8/setup-mysql.sql` 建库：

```bash
mysql -u root -p < PerFinManaSys.Web.Net8/setup-mysql.sql
```

### 2. 修改连接字符串
编辑 `PerFinManaSys.Web.Net8/appsettings.json` 中的 `ConnectionStrings.MoneyWatcherDB`：

```
Server=127.0.0.1;Port=3306;Database=PerFinManaSysDB;User=root;Password=你的密码;CharSet=utf8mb4;
```

### 3. EF Core 迁移建表

```bash
dotnet tool install --global dotnet-ef    # 首次需安装
cd PerFinManaSys.Web.Net8
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. 运行

```bash
dotnet run --project PerFinManaSys.Web.Net8
```

启动后访问 `/Login/Index` 进入登录页。详细 MySQL 配置见 [PerFinManaSys.Web.Net8/MYSQL_SETUP.md](PerFinManaSys.Web.Net8/MYSQL_SETUP.md)。

## 目录速览（.NET 8 版本）

```
PerFinManaSys.Web.Net8/
├── Auth/              # 认证与当前用户访问器
├── Controllers/       # 各业务控制器
├── Data/              # DbContext 与 DbInitializer（默认数据初始化）
├── Filters/           # 全局异常过滤器、压缩过滤器
├── Models/            # 实体：Users / Logins / Category / Income / Expenses
├── Tools/             # DES、Excel、Http、日志、验证码等工具类
├── Views/             # Razor 视图
└── wwwroot/           # 静态资源（css/js/images）
```

## 兼容性说明

旧版本 `PerFinManaSys.Web`（VS2013 + MVC4 + SQL Server 2012）已停止维护，仅保留作历史参考。新功能与问题修复均在 `PerFinManaSys.Web.Net8` 中进行。
