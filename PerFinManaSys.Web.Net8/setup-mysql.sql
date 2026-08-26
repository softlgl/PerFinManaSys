-- ============================================================
-- PerFinManaSys 升级到 .NET 8 + MySQL 建库脚本
-- 适用：从 SQL Server 切换到 MySQL（Code First 重建表，无旧数据）
-- ============================================================

-- 1) 创建数据库（utf8mb4 字符集，支持中文与 emoji）
CREATE DATABASE IF NOT EXISTS `PerFinManaSysDB`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_general_ci;

USE `PerFinManaSysDB`;

-- 2) 可选：为应用创建专用账号并授权（生产环境建议，不要用 root）
-- CREATE USER IF NOT EXISTS 'perfinfin'@'localhost' IDENTIFIED BY 'StrongPassword123!';
-- GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX ON `MoneyWatcherDB`.* TO 'perfinfin'@'localhost';
-- FLUSH PRIVILEGES;

-- 3) 表由 EF Core Code First 迁移自动创建，无需手建。
--    执行迁移命令（需先 dotnet restore 并安装 dotnet-ef 工具）：
--      dotnet tool install --global dotnet-ef
--      dotnet ef migrations add InitialCreate
--      dotnet ef database update
-- ============================================================
