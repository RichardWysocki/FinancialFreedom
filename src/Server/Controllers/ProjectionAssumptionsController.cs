using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/projection-assumptions")]
public class ProjectionAssumptionsController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<ProjectionAssumptionsDto>> Get(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var p = await db.ProjectionAssumptions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.HouseholdId == h.Id, cancellationToken);

        if (p is null)
            return Ok(new ProjectionAssumptionsDto
            {
                RetirementRateOfReturnPercent = 8,
                RetirementWithdrawalRatePercent = 4,
                HsaRateOfReturnPercent = 5,
                TaxableRateOfReturnPercent = 6,
                EducationRateOfReturnPercent = 5,
                EmergencyRateOfReturnPercent = 4,
                InflationRatePercent = 2.5m,
                LifeExpectancyAge = 95,
                ReplacementRateGreenThresholdPercent = 85,
                ReplacementRateYellowThresholdPercent = 70,
            });

        return Ok(new ProjectionAssumptionsDto
        {
            RetirementRateOfReturnPercent = p.RetirementRateOfReturnPercent,
            RetirementWithdrawalRatePercent = p.RetirementWithdrawalRatePercent,
            HsaRateOfReturnPercent = p.HsaRateOfReturnPercent,
            TaxableRateOfReturnPercent = p.TaxableRateOfReturnPercent,
            EducationRateOfReturnPercent = p.EducationRateOfReturnPercent,
            EmergencyRateOfReturnPercent = p.EmergencyRateOfReturnPercent,
            InflationRatePercent = p.InflationRatePercent,
            LifeExpectancyAge = p.LifeExpectancyAge,
            ReplacementRateGreenThresholdPercent = p.ReplacementRateGreenThresholdPercent,
            ReplacementRateYellowThresholdPercent = p.ReplacementRateYellowThresholdPercent,
        });
    }

    [HttpPut]
    public async Task<ActionResult> Put([FromBody] ProjectionAssumptionsDto body, CancellationToken cancellationToken)
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

        p.RetirementRateOfReturnPercent = body.RetirementRateOfReturnPercent;
        p.RetirementWithdrawalRatePercent = body.RetirementWithdrawalRatePercent;
        p.HsaRateOfReturnPercent = body.HsaRateOfReturnPercent;
        p.TaxableRateOfReturnPercent = body.TaxableRateOfReturnPercent;
        p.EducationRateOfReturnPercent = body.EducationRateOfReturnPercent;
        p.EmergencyRateOfReturnPercent = body.EmergencyRateOfReturnPercent;
        p.InflationRatePercent = body.InflationRatePercent;
        p.LifeExpectancyAge = body.LifeExpectancyAge;
        p.ReplacementRateGreenThresholdPercent = body.ReplacementRateGreenThresholdPercent;
        p.ReplacementRateYellowThresholdPercent = body.ReplacementRateYellowThresholdPercent;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
