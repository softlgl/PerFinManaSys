namespace PerFinManaSys.Web.Net8.Models;

public class Income
{
    public Guid I_ID { get; set; }
    public string? I_Title { get; set; }
    public string? I_Type { get; set; }
    public decimal I_Amount { get; set; }
    public DateTime I_Date { get; set; }
    public string? I_Remark { get; set; }
    public Guid I_UID { get; set; }
}
