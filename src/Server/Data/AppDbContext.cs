using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<FinancialGoal> FinancialGoals => Set<FinancialGoal>();
    public DbSet<PageVisitLog> PageVisitLogs => Set<PageVisitLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PageVisitLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.VisitedAt).IsRequired();
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.UserName).HasMaxLength(256);
            e.Property(x => x.PagePath).HasMaxLength(2048).IsRequired();
            e.Property(x => x.QueryString).HasMaxLength(2048);
            e.HasIndex(x => x.VisitedAt);
        });

        modelBuilder.Entity<FinancialGoal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TargetAmount).HasPrecision(18, 2);
            e.ToTable("FinancialGoals", t => t.ExcludeFromMigrations());
        });
    }
}
