namespace PerFinManaSys.Web.Net8.Auth;

/// <summary>
/// 当前登录用户访问器（替代原 FormsAuth.GetUserData / Request.IsAuthenticated）。
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    UserInfo? User { get; }
    bool IsAdmin { get; }
    Guid? UserId { get; }
    string? UserName { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public UserInfo? User
    {
        get
        {
            var p = Principal;
            if (p == null || !IsAuthenticated) return null;

            var idClaim = p.FindFirst("U_Id")?.Value;
            if (idClaim == null || !Guid.TryParse(idClaim, out var uid)) return null;

            var name = p.FindFirst("U_Name")?.Value;
            var realName = p.FindFirst("U_RealName")?.Value;

            return new UserInfo
            {
                U_ID = uid,
                U_Name = string.IsNullOrWhiteSpace(name) ? null : name,
                U_RealName = string.IsNullOrWhiteSpace(realName) ? null : realName,
                U_Role = int.TryParse(p.FindFirst("U_Role")?.Value, out var r) ? r : 1,
                U_IsAdmin = bool.TryParse(p.FindFirst("U_IsAdmin")?.Value, out var a) && a
            };
        }
    }

    public bool IsAdmin => User?.IsAdmin ?? false;
    public Guid? UserId => User?.U_ID;
    public string? UserName => User?.U_Name;
}
