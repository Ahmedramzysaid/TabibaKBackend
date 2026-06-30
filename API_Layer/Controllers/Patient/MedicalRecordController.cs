using System.Security.Claims;
using BusinessLayer.Interfaces;
using ClinicAPI.DTOs;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
public class MedicalRecordController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IFileService _fileService;

    public MedicalRecordController(IMedicalRecordService medicalRecordService, IFileService fileService)
    {
        _medicalRecordService = medicalRecordService;
        _fileService = fileService;
    }

    private string? GetDoctorIdForScope() => User.IsInRole(Roles.Doctor) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    [Authorize(Policy = AuthorizationPolicies.CanViewMedicalRecords)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<MedicalRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<MedicalRecordDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _medicalRecordService.GetAll(GetDoctorIdForScope(), pageNumber, pageSize);
        if (result.ErrorType != ServiceErrorType.Success)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(result.Message),
                ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };
        }
        return Ok(result.Data);
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewMedicalRecords)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalRecordDto>> GetById(Guid id)
    {
        var result = await _medicalRecordService.GetById(id, GetDoctorIdForScope());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewMedicalRecords)]
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalRecordDto>> GetByPatientId(string patientId)
    {
        var result = await _medicalRecordService.GetByPatientId(patientId);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }


    [Authorize(Policy = AuthorizationPolicies.CanDeleteMedicalRecord)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _medicalRecordService.Delete(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

}
