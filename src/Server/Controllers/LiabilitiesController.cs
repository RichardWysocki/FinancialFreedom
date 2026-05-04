using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/liabilities")]
public class LiabilitiesController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LiabilityDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var list = await db.Liabilities.AsNoTracking()
            .Where(l => l.HouseholdId == h.Id)
            .OrderBy(l => l.Name)
            .Select(l => new LiabilityDto(l.Id, l.Name, (int)l.LiabilityType, l.CurrentBalance))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<LiabilityDto>> Create([FromBody] LiabilityUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var l = new Liability
        {
            Id = Guid.NewGuid(),
            HouseholdId = h.Id,
            Name = body.Name.Trim(),
            LiabilityType = (LiabilityType)body.LiabilityType,
            CurrentBalance = body.CurrentBalance,
        };

        db.Liabilities.Add(l);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new LiabilityDto(l.Id, l.Name, (int)l.LiabilityType, l.CurrentBalance));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] LiabilityUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var l = await db.Liabilities.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (l is null)
            return NotFound();

        l.Name = body.Name.Trim();
        l.LiabilityType = (LiabilityType)body.LiabilityType;
        l.CurrentBalance = body.CurrentBalance;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var l = await db.Liabilities.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (l is null)
            return NotFound();

        await db.Accounts
            .Where(a => a.LinkedLiabilityId == id && a.HouseholdId == h.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.LinkedLiabilityId, (Guid?)null), cancellationToken);

        db.Liabilities.Remove(l);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
