using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/family-members")]
public class FamilyMembersController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FamilyMemberDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var list = await db.FamilyMembers.AsNoTracking()
            .Where(m => m.HouseholdId == h.Id)
            .OrderBy(m => m.DisplayOrder).ThenBy(m => m.Name)
            .Select(m => new FamilyMemberDto(m.Id, m.Name, (int)m.Type, m.DateOfBirth, m.DisplayOrder))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<FamilyMemberDto>> Create([FromBody] FamilyMemberUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var adults = await db.FamilyMembers.CountAsync(m => m.HouseholdId == h.Id && m.Type == FamilyMemberType.Adult, cancellationToken);
        if (body.Type == (int)FamilyMemberType.Adult && adults >= 2)
            return BadRequest("A maximum of two adults is allowed per household.");

        var m = new FamilyMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = h.Id,
            Name = body.Name.Trim(),
            Type = (FamilyMemberType)body.Type,
            DateOfBirth = body.DateOfBirth,
            DisplayOrder = body.DisplayOrder,
        };

        db.FamilyMembers.Add(m);
        if (m.Type == FamilyMemberType.Adult)
        {
            db.RetirementProfiles.Add(new RetirementProfile { FamilyMemberId = m.Id });
            db.HsaProfiles.Add(new HsaProfile { FamilyMemberId = m.Id });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new FamilyMemberDto(m.Id, m.Name, (int)m.Type, m.DateOfBirth, m.DisplayOrder));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] FamilyMemberUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (m is null)
            return NotFound();

        if (body.Type == (int)FamilyMemberType.Kid && m.Type == FamilyMemberType.Adult)
        {
            // downgrading adult to kid — ensure adult count
        }

        if (body.Type == (int)FamilyMemberType.Adult && m.Type == FamilyMemberType.Kid)
        {
            var otherAdults = await db.FamilyMembers.CountAsync(x => x.HouseholdId == h.Id && x.Type == FamilyMemberType.Adult && x.Id != m.Id, cancellationToken);
            if (otherAdults >= 2)
                return BadRequest("A maximum of two adults is allowed per household.");
        }

        m.Name = body.Name.Trim();
        m.Type = (FamilyMemberType)body.Type;
        m.DateOfBirth = body.DateOfBirth;
        m.DisplayOrder = body.DisplayOrder;

        if (m.Type == FamilyMemberType.Adult)
        {
            if (!await db.RetirementProfiles.AnyAsync(r => r.FamilyMemberId == m.Id, cancellationToken))
                db.RetirementProfiles.Add(new RetirementProfile { FamilyMemberId = m.Id });
            if (!await db.HsaProfiles.AnyAsync(r => r.FamilyMemberId == m.Id, cancellationToken))
                db.HsaProfiles.Add(new HsaProfile { FamilyMemberId = m.Id });
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var m = await db.FamilyMembers.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (m is null)
            return NotFound();

        await db.AccountOwners
            .Where(o => o.FamilyMemberId == id)
            .ExecuteDeleteAsync(cancellationToken);

        db.FamilyMembers.Remove(m);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
