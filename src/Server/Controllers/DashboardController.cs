using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/dashboard")]
public class DashboardController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get([FromQuery] string? owner, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var accounts = await db.Accounts.AsNoTracking()
            .Include(a => a.AssetCategory)
            .Include(a => a.Owners)
            .Where(a => a.HouseholdId == h.Id)
            .ToListAsync(cancellationToken);

        Guid? memberFilter = null;
        var jointOnly = string.Equals(owner, "joint", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(owner) && !jointOnly && Guid.TryParse(owner, out var gid))
            memberFilter = gid;

        var categoryTotals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        decimal total = 0;

        foreach (var a in accounts)
        {
            var amt = FilterAmount(a, memberFilter, jointOnly);
            if (amt == 0)
                continue;

            var cat = a.AssetCategory.Name;
            categoryTotals[cat] = categoryTotals.GetValueOrDefault(cat) + amt;
            total += amt;
        }

        var colors = new[] { "#1E88E5", "#00897B", "#FB8C00", "#E91E63", "#FDD835", "#7E57C2" };
        var list = new List<DashboardCategoryTotalDto>();
        var i = 0;
        foreach (var kv in categoryTotals.OrderByDescending(x => x.Value))
        {
            list.Add(new DashboardCategoryTotalDto(kv.Key, kv.Value, colors[i % colors.Length]));
            i++;
        }

        return Ok(new DashboardDto(total, list));
    }

    private static decimal FilterAmount(Account a, Guid? memberId, bool jointOnly)
    {
        if (jointOnly)
        {
            if (a.Owners.Count <= 1)
                return 0;
            return a.CurrentBalance;
        }

        if (memberId is null)
            return a.CurrentBalance;

        var pct = a.Owners.Where(o => o.FamilyMemberId == memberId).Sum(o => o.OwnershipPercent);
        return a.CurrentBalance * (pct / 100m);
    }
}
