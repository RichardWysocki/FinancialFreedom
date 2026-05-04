using FinancialFreedom.Server.Data;
using FinancialFreedom.Server.Services;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

/// <summary>What-if scenarios: temporarily override assumptions and compare projections.</summary>
[Route("api/what-if")]
public class WhatIfController(AppDbContext db, IFinancialProjectionService projections)
    : HouseholdControllerBase(db)
{
    [HttpPost("income-replacement")]
    public async Task<ActionResult<IncomeReplacementResultDto>> IncomeReplacement(
        [FromBody] ProjectionAssumptionsDto overrides,
        CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var p = await db.ProjectionAssumptions.FirstOrDefaultAsync(x => x.HouseholdId == h.Id, cancellationToken);
        if (p is null)
        {
            p = new ProjectionAssumptions { HouseholdId = h.Id };
            db.ProjectionAssumptions.Add(p);
        }

        var backup = (
            p.RetirementRateOfReturnPercent,
            p.RetirementWithdrawalRatePercent,
            p.HsaRateOfReturnPercent,
            p.TaxableRateOfReturnPercent,
            p.EducationRateOfReturnPercent,
            p.EmergencyRateOfReturnPercent,
            p.InflationRatePercent,
            p.LifeExpectancyAge,
            p.ReplacementRateGreenThresholdPercent,
            p.ReplacementRateYellowThresholdPercent);

        p.RetirementRateOfReturnPercent = overrides.RetirementRateOfReturnPercent;
        p.RetirementWithdrawalRatePercent = overrides.RetirementWithdrawalRatePercent;
        p.HsaRateOfReturnPercent = overrides.HsaRateOfReturnPercent;
        p.TaxableRateOfReturnPercent = overrides.TaxableRateOfReturnPercent;
        p.EducationRateOfReturnPercent = overrides.EducationRateOfReturnPercent;
        p.EmergencyRateOfReturnPercent = overrides.EmergencyRateOfReturnPercent;
        p.InflationRatePercent = overrides.InflationRatePercent;
        p.LifeExpectancyAge = overrides.LifeExpectancyAge;
        p.ReplacementRateGreenThresholdPercent = overrides.ReplacementRateGreenThresholdPercent;
        p.ReplacementRateYellowThresholdPercent = overrides.ReplacementRateYellowThresholdPercent;

        await db.SaveChangesAsync(cancellationToken);

        var result = await projections.GetIncomeReplacementAsync(h.Id, cancellationToken);

        (p.RetirementRateOfReturnPercent,
            p.RetirementWithdrawalRatePercent,
            p.HsaRateOfReturnPercent,
            p.TaxableRateOfReturnPercent,
            p.EducationRateOfReturnPercent,
            p.EmergencyRateOfReturnPercent,
            p.InflationRatePercent,
            p.LifeExpectancyAge,
            p.ReplacementRateGreenThresholdPercent,
            p.ReplacementRateYellowThresholdPercent) = backup;

        await db.SaveChangesAsync(cancellationToken);

        if (result is null)
            return BadRequest("Unable to compute replacement.");
        return Ok(result);
    }
}
