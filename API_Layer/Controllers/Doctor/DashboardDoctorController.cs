using DomainLayer.Constants;
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
public class DashboardDoctorController : ControllerBase
{
    private readonly IDashboardDoctorService _dashboardDoctorService;

    public DashboardDoctorController(IDashboardDoctorService dashboardDoctorService)
    {
        _dashboardDoctorService = dashboardDoctorService;
    }

    private string? GetCurrentDoctorId() => User.FindFirstValue(ClaimTypes.NameIdentifier);


    [HttpGet("overview")]
    [ProducesResponseType(typeof(DoctorDashboardOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDashboardOverviewDto>> GetOverview([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetOverviewAsync(id);
        return MapResult(result);
    }

    [HttpGet("today-appointments")]
    [ProducesResponseType(typeof(IEnumerable<DoctorTodayAppointmentItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorTodayAppointmentItemDto>>> GetTodayAppointments([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetTodayAppointmentsAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorTodayAppointmentItemDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpGet("next-appointment")]
    [ProducesResponseType(typeof(DoctorNextAppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorNextAppointmentDto>> GetNextAppointment([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetNextAppointmentAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => result.Data == null ? NoContent() : Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpGet("recent-patients")]
    [ProducesResponseType(typeof(IEnumerable<DoctorRecentPatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorRecentPatientDto>>> GetRecentPatients(
        [FromQuery] string? doctorId = null, [FromQuery] int count = 10)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        if (count < 1 || count > 50) count = 10;
        var result = await _dashboardDoctorService.GetRecentPatientsAsync(id, count);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorRecentPatientDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpPut("availability")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> SetAvailability([FromBody] SetAvailabilityRequest request, [FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.SetAvailabilityAsync(id, request.IsAvailable);
        return MapResult(result);
    }

    [HttpGet("scheduled-appointments")]
    [ProducesResponseType(typeof(IEnumerable<DoctorScheduledAppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorScheduledAppointmentDto>>> GetScheduledAppointments([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetScheduledAppointmentsAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorScheduledAppointmentDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpGet("past-medical-records")]
    [ProducesResponseType(typeof(IEnumerable<DoctorMedicalRecordSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetPastMedicalRecords([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetPastMedicalRecordsAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorMedicalRecordSummaryDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpGet("future-medical-records")]
    [ProducesResponseType(typeof(IEnumerable<DoctorMedicalRecordSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetFutureMedicalRecords([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetFutureMedicalRecordsAsync(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorMedicalRecordSummaryDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpPut("price")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrice([FromBody] SetPriceRequest request, [FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        if (request.Price < 0) return BadRequest("Price must be 0 or greater.");
        var result = await _dashboardDoctorService.SetPriceAsync(id, request.Price);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(new { message = "Price updated successfully.", price = request.Price }),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }


    [HttpPut("schedule")]
    [ProducesResponseType(typeof(IEnumerable<DoctorScheduleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorScheduleResponseDto>>> SetSchedule(
        [FromBody] List<SetDoctorScheduleDto> schedules, [FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.SetScheduleAsync(id, schedules);
        return MapResult(result);
    }

    [HttpGet("schedule")]
    [ProducesResponseType(typeof(IEnumerable<DoctorScheduleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorScheduleResponseDto>>> GetSchedule([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetScheduleAsync(id);
        return MapResult(result);
    }

    [HttpGet("appointments/not-completed")]
    [ProducesResponseType(typeof(PaginatedResult<DoctorTodayAppointmentItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<DoctorTodayAppointmentItemDto>>> GetNotCompletedAppointments(
        [FromQuery] string? doctorId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetNotCompletedAppointmentsAsync(id, page, pageSize);
        return MapResult(result);
    }

    [HttpGet("appointments/completed")]
    [ProducesResponseType(typeof(PaginatedResult<DoctorTodayAppointmentItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<DoctorTodayAppointmentItemDto>>> GetCompletedAppointments(
        [FromQuery] string? doctorId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetCompletedAppointmentsAsync(id, page, pageSize);
        return MapResult(result);
    }

    [HttpGet("medical-records/patient/{patientId}/completed")]
    [ProducesResponseType(typeof(IEnumerable<DoctorMedicalRecordSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetCompletedMedicalRecordsByPatient(
        [FromRoute] string patientId, [FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetMedicalRecordsByPatientAsync(id, patientId, completedOnly: true);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorMedicalRecordSummaryDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpGet("medical-records/patient/{patientId}/not-completed")]
    [ProducesResponseType(typeof(IEnumerable<DoctorMedicalRecordSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorMedicalRecordSummaryDto>>> GetNotCompletedMedicalRecordsByPatient(
        [FromRoute] string patientId, [FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetMedicalRecordsByPatientAsync(id, patientId, completedOnly: false);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data ?? Array.Empty<DoctorMedicalRecordSummaryDto>()),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [HttpGet("today-earnings")]
    [ProducesResponseType(typeof(DoctorTodayEarningsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorTodayEarningsDto>> GetTodayEarnings([FromQuery] string? doctorId = null)
    {
        var id = doctorId ?? GetCurrentDoctorId();
        if (string.IsNullOrEmpty(id)) return Unauthorized();
        var result = await _dashboardDoctorService.GetTodayEarningsAsync(id);
        return MapResult(result);
    }


    private ActionResult<T> MapResult<T>(Result<T> result) => result.ErrorType switch
    {
        ServiceErrorType.Success => Ok(result.Data),
        ServiceErrorType.NotFound => NotFound(result.Message),
        ServiceErrorType.ValidationError => BadRequest(result.Message),
        _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
    };
}

public class SetAvailabilityRequest
{
    public bool IsAvailable { get; set; }
}

public class SetPriceRequest
{
    public decimal Price { get; set; }
}
