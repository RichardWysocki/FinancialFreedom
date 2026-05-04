using FinancialFreedom.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Services;

public interface IHouseholdBootstrapService
{
    Task<Household> EnsureHouseholdForUserAsync(string userId, CancellationToken cancellationToken = default);
}

public class HouseholdBootstrapService(AppDbContext db) : IHouseholdBootstrapService
{
    public async Task<Household> EnsureHouseholdForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var existing = await db.Households
            .Include(h => h.AssetCategories)
            .Include(h => h.ProjectionAssumptions)
            .FirstOrDefaultAsync(h => h.OwnerUserId == userId, cancellationToken);

        if (existing is not null)
        {
            await EnsureDefaultCategoriesAsync(existing.Id, cancellationToken);
            await EnsureProjectionAssumptionsAsync(existing.Id, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return await db.Households
                .Include(h => h.AssetCategories)
                .Include(h => h.ProjectionAssumptions)
                .FirstAsync(h => h.Id == existing.Id, cancellationToken);
        }

        var id = Guid.NewGuid();
        var household = new Household
        {
            Id = id,
            OwnerUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ProjectionAssumptions = new ProjectionAssumptions { HouseholdId = id },
        };

        db.Households.Add(household);
        await db.SaveChangesAsync(cancellationToken);

        await SeedDefaultAssetCategoriesAsync(id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return await db.Households
            .Include(h => h.AssetCategories)
            .Include(h => h.ProjectionAssumptions)
            .FirstAsync(h => h.Id == id, cancellationToken);
    }

    private async Task EnsureProjectionAssumptionsAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var exists = await db.ProjectionAssumptions.AnyAsync(p => p.HouseholdId == householdId, cancellationToken);
        if (!exists)
            db.ProjectionAssumptions.Add(new ProjectionAssumptions { HouseholdId = householdId });
    }

    private async Task EnsureDefaultCategoriesAsync(Guid householdId, CancellationToken cancellationToken)
    {
        if (await db.AssetCategories.AnyAsync(c => c.HouseholdId == householdId, cancellationToken))
            return;

        await SeedDefaultAssetCategoriesAsync(householdId, cancellationToken);
    }

    private async Task SeedDefaultAssetCategoriesAsync(Guid householdId, CancellationToken cancellationToken)
    {
        if (await db.AssetCategories.AnyAsync(c => c.HouseholdId == householdId, cancellationToken))
            return;

        var defaults = new[]
        {
            ("Retirement", true, false, false, false, false),
            ("Health Savings Account", false, true, false, false, false),
            ("Emergency", false, false, false, true, false),
            ("Saving", false, false, false, false, false),
            ("Education", false, false, true, false, false),
        };

        var order = 0;
        foreach (var (name, isRet, isHsa, isEdu, isEmerg, isCash) in defaults)
        {
            db.AssetCategories.Add(new AssetCategory
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                Name = name,
                IsRetirement = isRet,
                IsHsa = isHsa,
                IsEducation = isEdu,
                IsEmergency = isEmerg,
                IsCash = isCash,
                DisplayOrder = order++,
            });
        }

        await Task.CompletedTask;
    }
}
