using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/asset-categories")]
public class AssetCategoriesController(AppDbContext db) : HouseholdControllerBase(db)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetCategoryDto>>> List(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var list = await db.AssetCategories.AsNoTracking()
            .Where(c => c.HouseholdId == h.Id)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new AssetCategoryDto(c.Id, c.Name, c.IsRetirement, c.IsHsa, c.IsEducation, c.IsEmergency, c.IsCash, c.DisplayOrder))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }
}
