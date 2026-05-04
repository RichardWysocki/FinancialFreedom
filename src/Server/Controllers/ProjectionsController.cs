using FinancialFreedom.Server.Data;
using FinancialFreedom.Server.Services;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FinancialFreedom.Server.Controllers;

[Route("api/projections")]
public class ProjectionsController(AppDbContext db, IFinancialProjectionService projections)
    : HouseholdControllerBase(db)
{
    [HttpGet("savings-by-age")]
    public async Task<ActionResult<SavingsByAgeResultDto>> SavingsByAge(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var r = await projections.GetSavingsByAgeAsync(h.Id, cancellationToken);
        if (r is null)
            return Ok(new SavingsByAgeResultDto(0, 0, 0, 0, 0, 0));
        return Ok(r);
    }

    [HttpGet("income-replacement")]
    public async Task<ActionResult<IncomeReplacementResultDto>> IncomeReplacement(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var r = await projections.GetIncomeReplacementAsync(h.Id, cancellationToken);
        if (r is null)
            return BadRequest("Add at least one adult with salary and retirement profile.");
        return Ok(r);
    }
}
