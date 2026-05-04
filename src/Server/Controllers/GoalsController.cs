using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/goals")]
public class GoalsController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GoalDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var list = await db.Goals.AsNoTracking()
            .Where(g => g.HouseholdId == h.Id)
            .OrderBy(g => g.Name)
            .Select(g => new GoalDto(g.Id, g.Name, g.TargetAmount, g.TargetDate, g.FundingAccountId))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<GoalDto>> Create([FromBody] GoalUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var g = new Goal
        {
            Id = Guid.NewGuid(),
            HouseholdId = h.Id,
            Name = body.Name.Trim(),
            TargetAmount = body.TargetAmount,
            TargetDate = body.TargetDate,
            FundingAccountId = body.FundingAccountId,
        };

        db.Goals.Add(g);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new GoalDto(g.Id, g.Name, g.TargetAmount, g.TargetDate, g.FundingAccountId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] GoalUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var g = await db.Goals.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (g is null)
            return NotFound();

        g.Name = body.Name.Trim();
        g.TargetAmount = body.TargetAmount;
        g.TargetDate = body.TargetDate;
        g.FundingAccountId = body.FundingAccountId;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var g = await db.Goals.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (g is null)
            return NotFound();

        db.Goals.Remove(g);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
