namespace PerFinManaSys.Web.Net8.Auth;

/// <summary>
/// 当前登录用户信息（从 Cookie Claims 还原，对应原 FormsAuth 序列化的 Users）。
/// </summary>
public class UserInfo
{
    public Guid U_ID { get; set; }
    public string? U_Name { get; set; }
    public string? U_RealName { get; set; }
    public int U_Role { get; set; }
    public bool U_IsAdmin { get; set; }

    /// <summary>是否管理员（角色 0 视为管理员）</summary>
    public bool IsAdmin => U_Role == (int)UserAuth.Admin;

    /// <summary>角色名：Admin / User</summary>
    public string RoleName => IsAdmin ? UserAuth.Admin.ToString() : UserAuth.User.ToString();
}
