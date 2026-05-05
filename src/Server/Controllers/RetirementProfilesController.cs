using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/retirement-profiles")]
public class RetirementProfilesController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RetirementProfileDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var adultIds = await db.FamilyMembers.AsNoTracking()
            .Where(m => m.HouseholdId == h.Id && m.Type == FamilyMemberType.Adult)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var inflation = await GetInflationPercentAsync(h.Id, cancellationToken);

        var profiles = await db.RetirementProfiles.AsNoTracking()
            .Include(r => r.FamilyMember)
            .Where(r => adultIds.Contains(r.FamilyMemberId))
            .OrderBy(r => r.FamilyMemberId)
            .ToListAsync(cancellationToken);

        var asOf = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var result = new List<RetirementProfileDto>(profiles.Count);
        foreach (var r in profiles)
        {
            var projected = RetirementSocialSecurityProjection.ProjectedMonthly(
                r.SocialSecurityMonthlyBenefit,
                inflation,
                r.FamilyMember.DateOfBirth,
                r.RetirementAge,
                asOf);
            result.Add(new RetirementProfileDto(
                r.FamilyMemberId,
                r.Salary,
                r.ContributionPercent,
                r.RetirementAge,
                r.EstimatedSalaryIncreasePercent,
                r.HasRetirementCatchup,
                r.CompanyMatchPercent,
                r.CompanyMatchEndsAtSalaryPercent,
                r.SocialSecurityMonthlyBenefit,
                projected));
        }

        return Ok(result);
    }

    [HttpPut("{familyMemberId:guid}")]
    public async Task<ActionResult> Put(Guid familyMemberId, [FromBody] RetirementProfileDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.Id == familyMemberId && x.HouseholdId == h.Id && x.Type == FamilyMemberType.Adult, cancellationToken);
        if (m is null)
            return NotFound();

        var r = await db.RetirementProfiles.FirstOrDefaultAsync(x => x.FamilyMemberId == familyMemberId, cancellationToken);
        if (r is null)
        {
            r = new RetirementProfile { FamilyMemberId = familyMemberId };
            db.RetirementProfiles.Add(r);
        }

        r.Salary = body.Salary;
        r.ContributionPercent = body.ContributionPercent;
        r.RetirementAge = body.RetirementAge;
        r.EstimatedSalaryIncreasePercent = body.EstimatedSalaryIncreasePercent;
        r.HasRetirementCatchup = body.HasRetirementCatchup;
        r.CompanyMatchPercent = body.CompanyMatchPercent;
        r.CompanyMatchEndsAtSalaryPercent = body.CompanyMatchEndsAtSalaryPercent;
        r.SocialSecurityMonthlyBenefit = body.SocialSecurityMonthlyBenefit;
        var inflation = await GetInflationPercentAsync(h.Id, cancellationToken);
        var asOf = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        r.ProjectedSocialSecurityMonthlyBenefit = RetirementSocialSecurityProjection.ProjectedMonthly(
            body.SocialSecurityMonthlyBenefit,
            inflation,
            m.DateOfBirth,
            body.RetirementAge,
            asOf);

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<decimal> GetInflationPercentAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var p = await db.ProjectionAssumptions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.HouseholdId == householdId, cancellationToken);
        return p?.InflationRatePercent ?? 2.5m;
    }
}
