using System.Security.Claims;
using FinancialFreedom.Server.Data;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PageVisitLogController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<PageVisitLogEntryDto>>> GetRecent(
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 500);
        var rows = await db.PageVisitLogs
            .AsNoTracking()
            .OrderByDescending(x => x.VisitedAt)
            .Take(take)
            .Select(x => new PageVisitLogEntryDto
            {
                Id = x.Id,
                VisitedAt = x.VisitedAt,
                UserId = x.UserId,
                UserName = x.UserName,
                PagePath = x.PagePath,
                QueryString = x.QueryString,
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] PageVisitLogRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PagePath))
            return BadRequest(new { message = "PagePath is required." });

        var path = request.PagePath.Trim();
        if (path.Length > 2048)
            path = path[..2048];

        var query = request.QueryString?.Trim();
        if (query is { Length: > 2048 })
            query = query[..2048];

        var user = User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = user.Identity?.Name
                       ?? user.FindFirstValue(ClaimTypes.Email)
                       ?? user.FindFirstValue(ClaimTypes.Name);

        db.PageVisitLogs.Add(new PageVisitLog
        {
            Id = Guid.NewGuid(),
            VisitedAt = DateTimeOffset.UtcNow,
            UserId = string.IsNullOrEmpty(userId) ? null : userId,
            UserName = string.IsNullOrEmpty(userName) ? null : userName,
            PagePath = path,
            QueryString = string.IsNullOrEmpty(query) ? null : query,
        });

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
