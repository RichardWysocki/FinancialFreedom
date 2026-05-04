using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<PageVisitLog> PageVisitLogs => Set<PageVisitLog>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<RetirementProfile> RetirementProfiles => Set<RetirementProfile>();
    public DbSet<HsaProfile> HsaProfiles => Set<HsaProfile>();
    public DbSet<ProjectionAssumptions> ProjectionAssumptions => Set<ProjectionAssumptions>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountOwner> AccountOwners => Set<AccountOwner>();
    public DbSet<AccountBalanceSnapshot> AccountBalanceSnapshots => Set<AccountBalanceSnapshot>();
    public DbSet<Liability> Liabilities => Set<Liability>();
    public DbSet<LiabilityBalanceSnapshot> LiabilityBalanceSnapshots => Set<LiabilityBalanceSnapshot>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<IrsLimit> IrsLimits => Set<IrsLimit>();
    public DbSet<SavingsByAgeMultiple> SavingsByAgeMultiples => Set<SavingsByAgeMultiple>();

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

        modelBuilder.Entity<Household>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            e.HasIndex(x => x.OwnerUserId).IsUnique();
            e.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FamilyMember>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.HouseholdId, x.DisplayOrder });
            e.HasOne(x => x.Household)
                .WithMany(h => h.FamilyMembers)
                .HasForeignKey(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RetirementProfile>(e =>
        {
            e.HasKey(x => x.FamilyMemberId);
            e.Property(x => x.Salary).HasPrecision(18, 2);
            e.Property(x => x.ContributionPercent).HasPrecision(9, 4);
            e.Property(x => x.EstimatedSalaryIncreasePercent).HasPrecision(9, 4);
            e.Property(x => x.CompanyMatchPercent).HasPrecision(9, 4);
            e.Property(x => x.CompanyMatchEndsAtSalaryPercent).HasPrecision(9, 4);
            e.Property(x => x.SocialSecurityMonthlyBenefit).HasPrecision(18, 2);
            e.HasOne(x => x.FamilyMember)
                .WithOne(m => m.RetirementProfile)
                .HasForeignKey<RetirementProfile>(x => x.FamilyMemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HsaProfile>(e =>
        {
            e.HasKey(x => x.FamilyMemberId);
            e.Property(x => x.AnnualHsaContribution).HasPrecision(18, 2);
            e.HasOne(x => x.FamilyMember)
                .WithOne(m => m.HsaProfile)
                .HasForeignKey<HsaProfile>(x => x.FamilyMemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectionAssumptions>(e =>
        {
            e.HasKey(x => x.HouseholdId);
            e.Property(x => x.RetirementRateOfReturnPercent).HasPrecision(9, 4);
            e.Property(x => x.RetirementWithdrawalRatePercent).HasPrecision(9, 4);
            e.Property(x => x.HsaRateOfReturnPercent).HasPrecision(9, 4);
            e.Property(x => x.TaxableRateOfReturnPercent).HasPrecision(9, 4);
            e.Property(x => x.EducationRateOfReturnPercent).HasPrecision(9, 4);
            e.Property(x => x.EmergencyRateOfReturnPercent).HasPrecision(9, 4);
            e.Property(x => x.InflationRatePercent).HasPrecision(9, 4);
            e.Property(x => x.ReplacementRateGreenThresholdPercent).HasPrecision(9, 4);
            e.Property(x => x.ReplacementRateYellowThresholdPercent).HasPrecision(9, 4);
            e.HasOne(x => x.Household)
                .WithOne(h => h.ProjectionAssumptions)
                .HasForeignKey<ProjectionAssumptions>(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AssetCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.HasIndex(x => new { x.HouseholdId, x.Name });
            e.HasOne(x => x.Household)
                .WithMany(h => h.AssetCategories)
                .HasForeignKey(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FinancialCompany).HasMaxLength(200).IsRequired();
            e.Property(x => x.AccountName).HasMaxLength(200).IsRequired();
            e.Property(x => x.CurrentBalance).HasPrecision(18, 2);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasOne(x => x.Household)
                .WithMany(h => h.Accounts)
                .HasForeignKey(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AssetCategory)
                .WithMany(c => c.Accounts)
                .HasForeignKey(x => x.AssetCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LinkedLiability)
                .WithMany(l => l.LinkedAccounts)
                .HasForeignKey(x => x.LinkedLiabilityId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AccountOwner>(e =>
        {
            e.HasKey(x => new { x.AccountId, x.FamilyMemberId });
            e.Property(x => x.OwnershipPercent).HasPrecision(9, 4);
            e.HasOne(x => x.Account)
                .WithMany(a => a.Owners)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.FamilyMember)
                .WithMany(m => m.AccountOwners)
                .HasForeignKey(x => x.FamilyMemberId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AccountBalanceSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Balance).HasPrecision(18, 2);
            e.HasIndex(x => new { x.AccountId, x.AsOfDate });
            e.HasOne(x => x.Account)
                .WithMany(a => a.BalanceSnapshots)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Liability>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CurrentBalance).HasPrecision(18, 2);
            e.HasOne(x => x.Household)
                .WithMany(h => h.Liabilities)
                .HasForeignKey(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LiabilityBalanceSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Balance).HasPrecision(18, 2);
            e.HasIndex(x => new { x.LiabilityId, x.AsOfDate });
            e.HasOne(x => x.Liability)
                .WithMany(l => l.BalanceSnapshots)
                .HasForeignKey(x => x.LiabilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Goal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TargetAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Household)
                .WithMany(h => h.Goals)
                .HasForeignKey(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.FundingAccount)
                .WithMany()
                .HasForeignKey(x => x.FundingAccountId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<IrsLimit>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.Year, x.LimitType }).IsUnique();
        });

        modelBuilder.Entity<SavingsByAgeMultiple>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Multiple).HasPrecision(9, 4);
            e.HasIndex(x => x.Age).IsUnique();
        });
    }
}
