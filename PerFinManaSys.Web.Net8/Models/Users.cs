namespace PerFinManaSys.Web.Net8.Models;

/// <summary>用户表（字段名对齐原 SQL Server 库实体，便于迁移与搬运）</summary>
public class Users
{
    public Guid U_ID { get; set; }
    public string? U_Name { get; set; }
    public string? U_PassWord { get; set; }
    public string? U_RealName { get; set; }
    public string? U_Tel { get; set; }
    public string? U_Email { get; set; }
    public int U_Role { get; set; }
    public bool U_IsAdmin { get; set; }
    public int U_Status { get; set; }
    public DateTime U_RegistDate { get; set; }
    public DateTime? U_LastLoginTime { get; set; }
}
