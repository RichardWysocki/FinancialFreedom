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

        var profiles = await db.RetirementProfiles.AsNoTracking()
            .Where(r => adultIds.Contains(r.FamilyMemberId))
            .ToListAsync(cancellationToken);

        var result = profiles
            .OrderBy(r => r.FamilyMemberId)
            .Select(r => new RetirementProfileDto(
                r.FamilyMemberId,
                r.Salary,
                r.ContributionPercent,
                r.RetirementAge,
                r.EstimatedSalaryIncreasePercent,
                r.HasRetirementCatchup,
                r.CompanyMatchPercent,
                r.CompanyMatchEndsAtSalaryPercent,
                r.SocialSecurityMonthlyBenefit))
            .ToList();

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

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
