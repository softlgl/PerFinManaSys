namespace PerFinManaSys.Web.Net8.Models;

public class Logins
{
    public Guid L_ID { get; set; }
    public Guid L_UID { get; set; }
    public string? L_UserName { get; set; }
    public string? L_HostName { get; set; }
    public string? L_IP { get; set; }
    public string? L_City { get; set; }
    public int L_Role { get; set; }
    public DateTime L_LoginTime { get; set; }
    public int L_Result { get; set; }
    public string? L_Remark { get; set; }
}
