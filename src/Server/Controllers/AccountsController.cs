using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/accounts")]
public class AccountsController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var accounts = await db.Accounts.AsNoTracking()
            .Include(a => a.Owners)
            .Where(a => a.HouseholdId == h.Id)
            .OrderBy(a => a.AccountName)
            .ToListAsync(cancellationToken);

        var result = new List<AccountDto>();
        foreach (var a in accounts)
        {
            result.Add(ToDto(a));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create([FromBody] AccountUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        if (!await db.AssetCategories.AnyAsync(c => c.Id == body.AssetCategoryId && c.HouseholdId == h.Id, cancellationToken))
            return BadRequest("Invalid asset category.");

        if (body.Owners.Count == 0)
            return BadRequest("At least one account owner is required.");

        var a = new Account
        {
            Id = Guid.NewGuid(),
            HouseholdId = h.Id,
            AssetCategoryId = body.AssetCategoryId,
            FinancialCompany = body.FinancialCompany.Trim(),
            AccountName = body.AccountName.Trim(),
            AssetClass = (AssetClass)body.AssetClass,
            TaxTreatment = (TaxTreatment)body.TaxTreatment,
            CurrentBalance = body.CurrentBalance,
            IncludeInRetirementProjections = body.IncludeInRetirementProjections,
            Notes = body.Notes,
            LinkedLiabilityId = body.LinkedLiabilityId,
        };

        foreach (var o in body.Owners)
        {
            if (!await db.FamilyMembers.AnyAsync(m => m.Id == o.FamilyMemberId && m.HouseholdId == h.Id, cancellationToken))
                return BadRequest("Invalid family member for ownership.");
            a.Owners.Add(new AccountOwner
            {
                AccountId = a.Id,
                FamilyMemberId = o.FamilyMemberId,
                OwnershipPercent = o.OwnershipPercent,
            });
        }

        db.Accounts.Add(a);
        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(a).Collection(x => x.Owners).LoadAsync(cancellationToken);
        return Ok(ToDto(a));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] AccountUpsertDto body, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var a = await db.Accounts
            .Include(x => x.Owners)
            .FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (a is null)
            return NotFound();

        if (!await db.AssetCategories.AnyAsync(c => c.Id == body.AssetCategoryId && c.HouseholdId == h.Id, cancellationToken))
            return BadRequest("Invalid asset category.");

        if (body.Owners.Count == 0)
            return BadRequest("At least one account owner is required.");

        a.AssetCategoryId = body.AssetCategoryId;
        a.FinancialCompany = body.FinancialCompany.Trim();
        a.AccountName = body.AccountName.Trim();
        a.AssetClass = (AssetClass)body.AssetClass;
        a.TaxTreatment = (TaxTreatment)body.TaxTreatment;
        a.CurrentBalance = body.CurrentBalance;
        a.IncludeInRetirementProjections = body.IncludeInRetirementProjections;
        a.Notes = body.Notes;
        a.LinkedLiabilityId = body.LinkedLiabilityId;

        db.AccountOwners.RemoveRange(a.Owners);
        a.Owners.Clear();

        foreach (var o in body.Owners)
        {
            if (!await db.FamilyMembers.AnyAsync(m => m.Id == o.FamilyMemberId && m.HouseholdId == h.Id, cancellationToken))
                return BadRequest("Invalid family member for ownership.");
            a.Owners.Add(new AccountOwner
            {
                AccountId = a.Id,
                FamilyMemberId = o.FamilyMemberId,
                OwnershipPercent = o.OwnershipPercent,
            });
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

        var a = await db.Accounts.FirstOrDefaultAsync(x => x.Id == id && x.HouseholdId == h.Id, cancellationToken);
        if (a is null)
            return NotFound();

        await db.Goals
            .Where(g => g.FundingAccountId == id && g.HouseholdId == h.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.FundingAccountId, (Guid?)null), cancellationToken);

        db.Accounts.Remove(a);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static AccountDto ToDto(Account a) =>
        new(
            a.Id,
            a.AssetCategoryId,
            a.FinancialCompany,
            a.AccountName,
            (int)a.AssetClass,
            (int)a.TaxTreatment,
            a.CurrentBalance,
            a.IncludeInRetirementProjections,
            a.Notes,
            a.LinkedLiabilityId,
            a.Owners.Select(o => new AccountOwnerDto(o.FamilyMemberId, o.OwnershipPercent)).ToList());
}
