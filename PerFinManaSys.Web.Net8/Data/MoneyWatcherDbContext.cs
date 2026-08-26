using Microsoft.EntityFrameworkCore;
using PerFinManaSys.Web.Net8.Models;

namespace PerFinManaSys.Web.Net8.Data;

public class MoneyWatcherDbContext : DbContext
{
    public MoneyWatcherDbContext(DbContextOptions<MoneyWatcherDbContext> options)
        : base(options)
    {
    }

    public DbSet<Users> Users { get; set; } = null!;
    public DbSet<Income> Income { get; set; } = null!;
    public DbSet<Category> Category { get; set; } = null!;
    public DbSet<Logins> Logins { get; set; } = null!;
    public DbSet<Expenses> Expenses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet("utf8mb4", DelegationModes.ApplyToAll);
        modelBuilder.UseCollation("utf8mb4_general_ci");

        ConfigureUsers(modelBuilder);
        ConfigureIncome(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureLogins(modelBuilder);
        ConfigureExpenses(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Users>();
        e.ToTable("Users");
        e.HasKey(x => x.U_ID);
        e.Property(x => x.U_ID).HasColumnType("char(36)");
        e.Property(x => x.U_Name).HasColumnType("varchar(50)").IsRequired(false);
        e.Property(x => x.U_PassWord).HasColumnType("varchar(200)").IsRequired(false);
        e.Property(x => x.U_RealName).HasColumnType("varchar(50)").IsRequired(false);
        e.Property(x => x.U_Tel).HasColumnType("varchar(100)").IsRequired(false);
        e.Property(x => x.U_Email).HasColumnType("varchar(100)").IsRequired(false);
        e.Property(x => x.U_Role);
        e.Property(x => x.U_IsAdmin);
        e.Property(x => x.U_Status);
        e.Property(x => x.U_RegistDate).HasColumnType("datetime");
        e.Property(x => x.U_LastLoginTime).HasColumnType("datetime").IsRequired(false);
        e.HasIndex(x => x.U_Name);
    }

    private static void ConfigureIncome(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Income>();
        e.ToTable("Income");
        e.HasKey(x => x.I_ID);
        e.Property(x => x.I_ID).HasColumnType("char(36)");
        e.Property(x => x.I_Title).HasColumnType("varchar(100)").IsRequired(false);
        e.Property(x => x.I_Type).HasColumnType("varchar(50)").IsRequired(false);
        e.Property(x => x.I_Amount).HasPrecision(18, 2);
        e.Property(x => x.I_Date).HasColumnType("datetime");
        e.Property(x => x.I_Remark).HasColumnType("varchar(500)").IsRequired(false);
        e.Property(x => x.I_UID).HasColumnType("char(36)");
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Category>();
        e.ToTable("Category");
        e.HasKey(x => x.C_ID);
        e.Property(x => x.C_ID).HasColumnType("char(36)");
        e.Property(x => x.C_Name).HasColumnType("varchar(50)").IsRequired(false);
        e.Property(x => x.C_Type);
        e.Property(x => x.C_UsersID).HasColumnType("char(36)");
    }

    private static void ConfigureLogins(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Logins>();
        e.ToTable("Logins");
        e.HasKey(x => x.L_ID);
        e.Property(x => x.L_ID).HasColumnType("char(36)");
        e.Property(x => x.L_UID).HasColumnType("char(36)");
        e.Property(x => x.L_UserName).HasColumnType("varchar(50)").IsRequired(false);
        e.Property(x => x.L_HostName).HasColumnType("varchar(100)").IsRequired(false);
        e.Property(x => x.L_IP).HasColumnType("varchar(50)").IsRequired(false);
        e.Property(x => x.L_City).HasColumnType("varchar(100)").IsRequired(false);
        e.Property(x => x.L_Role);
        e.Property(x => x.L_LoginTime).HasColumnType("datetime");
        e.Property(x => x.L_Result);
        e.Property(x => x.L_Remark).HasColumnType("varchar(255)").IsRequired(false);
    }

    private static void ConfigureExpenses(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Expenses>();
        e.ToTable("Expenses");
        e.HasKey(x => x.E_ID);
        e.Property(x => x.E_ID).HasColumnType("char(36)");
        e.Property(x => x.E_Title).HasColumnType("varchar(100)").IsRequired(false);
        e.Property(x => x.E_Type).HasColumnType("char(36)");
        e.Property(x => x.E_Amount).HasPrecision(18, 2);
        e.Property(x => x.E_Date).HasColumnType("datetime");
        e.Property(x => x.E_Remark).HasColumnType("varchar(500)").IsRequired(false);
        e.Property(x => x.E_UID).HasColumnType("char(36)");
    }
}
