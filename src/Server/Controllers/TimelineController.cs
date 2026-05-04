using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/timeline")]
public class TimelineController(AppDbContext db) : HouseholdControllerBase(db)
{
    /// <summary>Aggregated account snapshots by date and category for stacked charts.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimelinePointDto>>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var raw = await (
                from s in db.AccountBalanceSnapshots.AsNoTracking()
                join a in db.Accounts.AsNoTracking() on s.AccountId equals a.Id
                join c in db.AssetCategories.AsNoTracking() on a.AssetCategoryId equals c.Id
                where a.HouseholdId == h.Id
                select new { s.AsOfDate, CategoryName = c.Name, s.Balance })
            .ToListAsync(cancellationToken);

        var filtered = raw.Where(x =>
            (from is null || x.AsOfDate >= from) &&
            (to is null || x.AsOfDate <= to)).ToList();

        var grouped = filtered
            .GroupBy(x => new { x.AsOfDate, x.CategoryName })
            .Select(g => new TimelinePointDto(g.Key.AsOfDate, g.Key.CategoryName, g.Sum(x => x.Balance)))
            .OrderBy(x => x.Date)
            .ToList();

        return Ok(grouped);
    }
}
