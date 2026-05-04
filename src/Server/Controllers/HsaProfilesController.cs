using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/hsa-profiles")]
public class HsaProfilesController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HsaProfileDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var memberIds = await db.FamilyMembers.AsNoTracking()
            .Where(m => m.HouseholdId == h.Id && m.Type == FamilyMemberType.Adult)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var list = await db.HsaProfiles.AsNoTracking()
            .Where(p => memberIds.Contains(p.FamilyMemberId))
            .Join(db.FamilyMembers.AsNoTracking(), p => p.FamilyMemberId, m => m.Id, (p, m) => new { p, m.Name })
            .OrderBy(x => x.Name)
            .Select(x => new HsaProfileDto(x.p.FamilyMemberId, x.p.HasHsa, x.p.AnnualHsaContribution, x.p.HasHsaCatchup))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPut("{familyMemberId:guid}")]
    public async Task<ActionResult> Put(Guid familyMemberId, [FromBody] HsaProfileDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.Id == familyMemberId && x.HouseholdId == h.Id && x.Type == FamilyMemberType.Adult, cancellationToken);
        if (m is null)
            return NotFound();

        var p = await db.HsaProfiles.FirstOrDefaultAsync(x => x.FamilyMemberId == familyMemberId, cancellationToken);
        if (p is null)
        {
            p = new HsaProfile { FamilyMemberId = familyMemberId };
            db.HsaProfiles.Add(p);
        }

        p.HasHsa = body.HasHsa;
        p.AnnualHsaContribution = body.AnnualHsaContribution;
        p.HasHsaCatchup = body.HasHsaCatchup;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
