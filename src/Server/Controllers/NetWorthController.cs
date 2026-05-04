using FinancialFreedom.Business.Projections;
using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/net-worth")]
public class NetWorthController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet("summary")]
    public async Task<ActionResult<NetWorthSummaryDto>> Summary(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var priorYear = new DateOnly(today.Year - 1, 1, 1);

        var accounts = await db.Accounts.AsNoTracking()
            .Include(a => a.AssetCategory)
            .Include(a => a.Owners)
            .Where(a => a.HouseholdId == h.Id)
            .ToListAsync(cancellationToken);

        var liabilities = await db.Liabilities.AsNoTracking()
            .Where(l => l.HouseholdId == h.Id)
            .ToListAsync(cancellationToken);

        var goals = await db.Goals.AsNoTracking()
            .Where(g => g.HouseholdId == h.Id)
            .ToListAsync(cancellationToken);

        var snapshotIds = accounts.Select(a => a.Id).ToList();
        var priorBalances = await db.AccountBalanceSnapshots.AsNoTracking()
            .Where(s => snapshotIds.Contains(s.AccountId) && s.AsOfDate <= priorYear)
            .GroupBy(s => s.AccountId)
            .Select(g => new { AccountId = g.Key, Balance = g.OrderByDescending(x => x.AsOfDate).First().Balance })
            .ToDictionaryAsync(x => x.AccountId, x => x.Balance, cancellationToken);

        var liquid = accounts.Where(a => a.AssetClass == AssetClass.Liquid).ToList();
        var semi = accounts.Where(a => a.AssetClass == AssetClass.SemiLiquid).ToList();
        var totalLiquid = liquid.Sum(a => a.CurrentBalance);
        var totalSemi = semi.Sum(a => a.CurrentBalance);
        var grossAssets = totalLiquid + totalSemi;

        var assetRows = new List<NetWorthRowDto>();
        foreach (var a in liquid.OrderByDescending(x => x.CurrentBalance))
        {
            var prior = priorBalances.GetValueOrDefault(a.Id);
            var pct = grossAssets > 0 ? a.CurrentBalance / grossAssets * 100 : 0;
            decimal? yoy = prior > 0 ? (a.CurrentBalance - prior) / prior * 100 : null;
            assetRows.Add(new NetWorthRowDto(
                a.AccountName,
                a.AssetCategory.Name,
                a.CurrentBalance,
                Math.Round(pct, 1),
                prior > 0 ? prior : null,
                yoy is null ? null : Math.Round(yoy.Value, 1)));
        }

        var semiRows = semi
            .Select(a => new NetWorthRowDto(a.AccountName, a.AssetCategory.Name, a.CurrentBalance, grossAssets > 0 ? Math.Round(a.CurrentBalance / grossAssets * 100, 1) : 0, null, null))
            .ToList();

        var liabTotal = liabilities.Sum(l => l.CurrentBalance);
        var goalTotal = goals.Sum(g => g.TargetAmount);

        var liabilityRows = liabilities
            .Select(l => new NetWorthRowDto(l.Name, "Liability", l.CurrentBalance, 0, null, null))
            .ToList();

        var goalRows = goals
            .Select(g => new NetWorthRowDto(g.Name, "Goal", g.TargetAmount, 0, null, null))
            .ToList();

        var inputs = accounts.Select(a => new AccountBalanceInput(
            a.Id,
            a.AccountName,
            a.AssetCategory.Name,
            (int)a.AssetClass,
            (int)a.TaxTreatment,
            a.CurrentBalance,
            a.Owners.Select(o => (o.FamilyMemberId, string.Empty, o.OwnershipPercent)).ToList())).ToList();

        var nw = NetWorthCalculator.Compute(inputs, liabTotal, goalTotal);

        var retirementSlice = accounts
            .Where(a => a.AssetCategory.IsRetirement)
            .GroupBy(a => a.TaxTreatment)
            .Select(g => new DashboardCategoryTotalDto(
                g.Key.ToString(),
                g.Sum(x => x.CurrentBalance),
                null))
            .ToList();

        return Ok(new NetWorthSummaryDto(
            today,
            grossAssets,
            liabTotal,
            nw.NetWorthLiquid,
            nw.NetWorthIncludingRealEstate,
            assetRows,
            semiRows,
            liabilityRows,
            goalRows,
            retirementSlice));
    }
}
