using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[AllowAnonymous]
[Route("api/reference")]
public class ReferenceDataController(AppDbContext db) : ControllerBase
{
    [HttpGet("irs-limits")]
    public async Task<ActionResult<IReadOnlyList<IrsLimitDto>>> IrsLimits([FromQuery] int? year, CancellationToken cancellationToken)
    {
        var q = db.IrsLimits.AsNoTracking().AsQueryable();
        if (year is not null)
            q = q.Where(x => x.Year == year);

        var list = await q.OrderBy(x => x.Year).ThenBy(x => x.LimitType)
            .Select(x => new IrsLimitDto(x.Year, (int)x.LimitType, x.Amount))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("savings-by-age")]
    public async Task<ActionResult<IReadOnlyList<SavingsByAgeMultipleDto>>> SavingsByAge(CancellationToken cancellationToken)
    {
        var list = await db.SavingsByAgeMultiples.AsNoTracking()
            .OrderBy(x => x.Age)
            .Select(x => new SavingsByAgeMultipleDto(x.Age, x.Multiple))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }
}
