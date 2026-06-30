using System.Security.Claims;
using AutoMapper;
using BusinessLayer.Validations;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
[Produces("application/json")]
[Consumes("application/json")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IMapper _mapper;

    public AppointmentController(IAppointmentService appointmentService, IMapper mapper)
    {
        _appointmentService = appointmentService;
        _mapper = mapper;
    }

    [Authorize(Policy = AuthorizationPolicies.CanCreateAppointment)]
    [HttpPost]
    [ProducesResponseType(typeof(CreateAppointmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CreateAppointmentResponseDto>> Add([FromBody] CreateAppointmentDto createDto)
    {
        var validator = new CreateAppointmentDtoValidator();
        var validationResult = await validator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return BadRequest(message);
        }
        var appointmentDto = _mapper.Map<AppointmentDto>(createDto);
        var result = await _appointmentService.Add(appointmentDto);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => CreatedAtAction(nameof(Add), new { id = result.Data!.AppointmentID }, result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanRescheduleAppointment)]
    [HttpPut("reschedule-appointment")]
    [ProducesResponseType(typeof(CreateAppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateAppointmentResponseDto>> RescheduleAppointment([FromBody] RescheduleAppointmentRequestDto request)
    {
        var rescheduleDto = new RescheduleAppointmentDto
        {
            AppointmentID = request.AppointmentID,
            NewAppointmentDate = request.NewAppointmentDate,
            NewAppointmentTime = request.NewAppointmentTime,
            DoctorID = null
        };
        var result = await _appointmentService.Reschedule(rescheduleDto);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanCancelAppointment)]
    [HttpDelete("cancel-appointment/{appointmentId}")]
    [ProducesResponseType(typeof(CreateAppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateAppointmentResponseDto>> CancelAppointment([FromRoute] Guid appointmentId)
    {
        var result = await _appointmentService.Cancel(appointmentId);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanCompleteAppointment)]
    [HttpPut("complete-appointment")]
    [ProducesResponseType(typeof(CreateAppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateAppointmentResponseDto>> CompleteAppointment([FromBody] CompleteAppointmentRequestDto request)
    {
        var result = await _appointmentService.CompleteByAppointmentId(request.AppointmentID);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewAppointments)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<CreateAppointmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<CreateAppointmentResponseDto>>> Get(
        [FromQuery] string? patientId = null,
        [FromQuery] string? doctorId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _appointmentService.GetAppointmentsAsCreateResponse(patientId, doctorId, pageNumber, pageSize);
        if (result.ErrorType != ServiceErrorType.Success)
            return result.ErrorType == ServiceErrorType.NotFound ? NotFound(result.Message) : StatusCode(StatusCodes.Status500InternalServerError, result.Message);
        return Ok(result.Data);
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewAppointments)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CreateAppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateAppointmentResponseDto>> GetById([FromRoute] Guid id)
    {
        var result = await _appointmentService.GetByIdAsCreateResponse(id);
        if (result.ErrorType != ServiceErrorType.Success)
            return result.ErrorType == ServiceErrorType.NotFound ? NotFound(result.Message) : StatusCode(StatusCodes.Status500InternalServerError, result.Message);
        return Ok(result.Data);
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewAppointments)]
    [HttpPost("rate")]
    [ProducesResponseType(typeof(CreateAppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateAppointmentResponseDto>> SubmitRating(SubmitDoctorRatingDto dto)
    {
        var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();

        var result = await _appointmentService.SubmitRating(dto, patientId);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(new { message = "Thanks for your rating!", appointment = result.Data }),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }
}
