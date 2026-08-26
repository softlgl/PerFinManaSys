namespace PerFinManaSys.Web.Net8.Models;

public class Expenses
{
    public Guid E_ID { get; set; }
    public string? E_Title { get; set; }
    public Guid E_Type { get; set; }
    public decimal E_Amount { get; set; }
    public DateTime E_Date { get; set; }
    public string? E_Remark { get; set; }
    public Guid E_UID { get; set; }
}
