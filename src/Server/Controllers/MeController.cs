using System.Security.Claims;
using FinancialFreedom.Server.Data;
using FinancialFreedom.Server.Services;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinancialFreedom.Server.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController(
    UserManager<ApplicationUser> userManager,
    IHouseholdBootstrapService householdBootstrap) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CurrentUserProfileDto>> Get(CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
            return Unauthorized();

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var household = await householdBootstrap.EnsureHouseholdForUserAsync(id, cancellationToken);

        return Ok(new CurrentUserProfileDto
        {
            HouseholdId = household.Id,
            Email = user.Email,
            UserName = user.UserName,
            LastLoginAt = user.LastLoginAt,
        });
    }
}
