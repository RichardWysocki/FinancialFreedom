using FinancialFreedom.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/snapshots")]
public class SnapshotsController(AppDbContext db) : HouseholdControllerBase(db)
{
    /// <summary>Creates balance snapshots for all accounts and liabilities as of today (SaveData).</summary>
    [HttpPost("save-all")]
    public async Task<ActionResult> SaveAll(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var accounts = await db.Accounts.Where(a => a.HouseholdId == h.Id).ToListAsync(cancellationToken);
        foreach (var a in accounts)
        {
            db.AccountBalanceSnapshots.Add(new AccountBalanceSnapshot
            {
                Id = Guid.NewGuid(),
                AccountId = a.Id,
                AsOfDate = today,
                Balance = a.CurrentBalance,
                Source = SnapshotSource.SaveData,
            });
        }

        var liabilities = await db.Liabilities.Where(l => l.HouseholdId == h.Id).ToListAsync(cancellationToken);
        foreach (var l in liabilities)
        {
            db.LiabilityBalanceSnapshots.Add(new LiabilityBalanceSnapshot
            {
                Id = Guid.NewGuid(),
                LiabilityId = l.Id,
                AsOfDate = today,
                Balance = l.CurrentBalance,
                Source = SnapshotSource.SaveData,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
