# MySQL 安装与建库指南（Windows）

本项目从原 SQL Server 迁移到 **MySQL 8.x**，使用 `Pomelo.EntityFrameworkCore.MySql` 提供程序，采用 **EF Core Code First** 方式建表（不迁移旧数据）。

## 1. 安装 MySQL
- 下载 MySQL Installer for Windows：https://dev.mysql.com/downloads/installer/
- 选择 "Developer Default" 或单独安装 `MySQL Server 8.x` + `MySQL Workbench`
- 记住 root 密码

## 2. 准备数据库
执行 `setup-mysql.sql`（用 MySQL Workbench 或命令行）：
```
mysql -u root -p < setup-mysql.sql
```

## 3. 修改连接字符串
编辑 `appsettings.json` 中 `ConnectionStrings.MoneyWatcherDB`，填入你的 host/port/user/password：
```
Server=localhost;Port=3306;Database=MoneyWatcherDB;User=root;Password=你的密码;CharSet=utf8mb4;
```

## 4. 用 EF Core 迁移建表
```
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```
迁移会在 `MoneyWatcherDB` 中自动创建 `Users / Logins / Category / Income / Expenses` 等表。

## 5. 运行
```
dotnet run --project PerFinManaSys.Web.Net8
```
首次访问 `/Login/Index` 即可。若需初始管理员账号，可在 `LoginController` 的 Seed 逻辑或手动 INSERT 一条 `Users` 记录（密码经 DES 加密）。
