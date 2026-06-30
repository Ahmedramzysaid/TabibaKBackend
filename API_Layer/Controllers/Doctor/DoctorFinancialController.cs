using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DoctorFinancialController : ControllerBase
{
    private readonly IDashboardDoctorService _dashboardDoctorService;

    public DoctorFinancialController(IDashboardDoctorService dashboardDoctorService)
    {
        _dashboardDoctorService = dashboardDoctorService;
    }

    private string? GetCurrentDoctorId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("report")]
    [ProducesResponseType(typeof(DoctorFinancialReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorFinancialReportDto>> GetFinancialReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id))
            return Unauthorized();

        var result = await _dashboardDoctorService.GetFinancialReportAsync(id, from, to);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }
}
