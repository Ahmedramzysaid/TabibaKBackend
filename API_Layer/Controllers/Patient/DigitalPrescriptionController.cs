using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class DigitalPrescriptionController : ControllerBase
{
    private readonly IDigitalPrescriptionService _service;

    public DigitalPrescriptionController(IDigitalPrescriptionService service)
    {
        _service = service;
    }

    private string? GetDoctorId() => User.IsInRole(Roles.Doctor) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
    private string? GetPatientId() => User.IsInRole(Roles.Patient) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<DigitalPrescriptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<DigitalPrescriptionDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAll(GetDoctorId(), GetPatientId(), pageNumber, pageSize);
        if (result.ErrorType != ServiceErrorType.Success && result.ErrorType != ServiceErrorType.NotFound)
            return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
        return Ok(result.Data);
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DigitalPrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DigitalPrescriptionDto>> GetById(Guid id)
    {
        var result = await _service.GetById(id, GetDoctorId(), GetPatientId());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet("medical-record/{medicalRecordId}")]
    [ProducesResponseType(typeof(IEnumerable<DigitalPrescriptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DigitalPrescriptionDto>>> GetByMedicalRecordId(Guid medicalRecordId)
    {
        var result = await _service.GetByMedicalRecordId(medicalRecordId, GetPatientId());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(typeof(IEnumerable<DigitalPrescriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DigitalPrescriptionDto>>> GetByPatientId(string patientId)
    {
        var currentPatientId = GetPatientId();
        if (!string.IsNullOrEmpty(currentPatientId) && currentPatientId != patientId)
            return Forbid();
        var result = await _service.GetByPatientId(patientId, GetDoctorId());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanCreateDigitalPrescription)]
    [HttpPost]
    [ProducesResponseType(typeof(DigitalPrescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DigitalPrescriptionDto>> Create([FromBody] CreateDigitalPrescriptionDto dto)
    {
        if (dto == null) return BadRequest();
        var result = await _service.Create(dto, GetDoctorId());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => CreatedAtAction(nameof(GetById), new { id = result.Data!.DigitalPrescriptionId }, result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanEditDigitalPrescription)]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DigitalPrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DigitalPrescriptionDto>> Update(Guid id, [FromBody] CreateDigitalPrescriptionDto dto)
    {
        if (dto == null) return BadRequest();
        var result = await _service.Update(id, dto, GetDoctorId());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanDeleteDigitalPrescription)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.Delete(id, GetDoctorId());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }
}
