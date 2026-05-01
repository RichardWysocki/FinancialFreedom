using FinancialFreedom.Business;
using FinancialFreedom.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FinancialFreedom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FutureSalaryController : ControllerBase
{
    [HttpPost("project")]
    [ProducesResponseType(typeof(FutureSalaryProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<FutureSalaryProjectionResponse> Project([FromBody] FutureSalaryProjectionRequest request)
    {
        var calendarStartYear = request.CalendarStartYear ?? DateTime.UtcNow.Year;
        var result = FutureSalaryCalculator.TryProject(request, calendarStartYear, out var errorMessage);
        if (result is null)
            return BadRequest(new ApiErrorResponse { Message = errorMessage });

        return Ok(result);
    }
}
