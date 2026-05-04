using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Data;

public static class ReferenceDataSeeder
{
    public static async Task SeedGlobalReferenceDataAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.SavingsByAgeMultiples.AnyAsync(cancellationToken))
        {
            db.SavingsByAgeMultiples.AddRange(
                new SavingsByAgeMultiple { Age = 30, Multiple = 1m },
                new SavingsByAgeMultiple { Age = 35, Multiple = 2m },
                new SavingsByAgeMultiple { Age = 40, Multiple = 3m },
                new SavingsByAgeMultiple { Age = 45, Multiple = 4m },
                new SavingsByAgeMultiple { Age = 50, Multiple = 6m },
                new SavingsByAgeMultiple { Age = 55, Multiple = 7m },
                new SavingsByAgeMultiple { Age = 60, Multiple = 8m },
                new SavingsByAgeMultiple { Age = 67, Multiple = 10m });
        }

        if (!await db.IrsLimits.AnyAsync(cancellationToken))
        {
            // Approximate common limits for projection tests (2024–2026).
            foreach (var year in new[] { 2024, 2025, 2026 })
            {
                var (hsaS, hsaF, hsaCatch, k401, k401Catch, ira, iraCatch, ssWage) = year switch
                {
                    2024 => (4150m, 8300m, 1000m, 23000m, 7500m, 7000m, 1000m, 168600m),
                    2025 => (4300m, 8550m, 1000m, 23500m, 7500m, 7000m, 1000m, 176100m),
                    _ => (4450m, 8900m, 1000m, 24000m, 7500m, 7000m, 1000m, 176100m),
                };

                db.IrsLimits.AddRange(
                    new IrsLimit { Year = year, LimitType = IrsLimitType.HsaSingle, Amount = hsaS },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.HsaFamily, Amount = hsaF },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.HsaCatchup55, Amount = hsaCatch },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.Plan401kElectiveDeferral, Amount = k401 },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.Plan401kCatchup50, Amount = k401Catch },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.IraContribution, Amount = ira },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.IraCatchup50, Amount = iraCatch },
                    new IrsLimit { Year = year, LimitType = IrsLimitType.SocialSecurityWageBase, Amount = ssWage });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
