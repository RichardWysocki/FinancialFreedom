using System.Security.Claims;
using FinancialFreedom.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[ApiController]
[Authorize]
public abstract class HouseholdControllerBase(AppDbContext db) : ControllerBase
{
    protected string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected async Task<Household?> GetHouseholdAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(UserId))
            return null;

        return await db.Households
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.OwnerUserId == UserId, cancellationToken);
    }

    protected async Task<Household?> GetHouseholdTrackedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(UserId))
            return null;

        return await db.Households
            .FirstOrDefaultAsync(h => h.OwnerUserId == UserId, cancellationToken);
    }
}
