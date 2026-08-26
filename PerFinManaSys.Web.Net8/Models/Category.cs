namespace PerFinManaSys.Web.Net8.Models;

public class Category
{
    public Guid C_ID { get; set; }
    public string? C_Name { get; set; }
    public int C_Type { get; set; }
    public Guid C_UsersID { get; set; }
}
