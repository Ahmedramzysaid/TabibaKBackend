using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using ClinicAPI.DTOs;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IFileService _fileService;

    public PrescriptionController(IPrescriptionService prescriptionService, IFileService fileService)
    {
        _prescriptionService = prescriptionService;
        _fileService = fileService;
    }

    private string? GetDoctorIdForScope() => User.IsInRole(Roles.Doctor) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    [Authorize(Policy = AuthorizationPolicies.CanCreatePrescription)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrescriptionDto>> Add([FromForm] CreatePrescriptionWithFileDto createPrescriptionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string? filePath = null;
        string? fileType = null;

        if (createPrescriptionDto.File != null && createPrescriptionDto.File.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(createPrescriptionDto.File.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Only PDF and image files are allowed.");

            if (createPrescriptionDto.File.Length > 10 * 1024 * 1024)
                return BadRequest("File size exceeds maximum allowed size of 10MB");

            if (fileExtension == ".pdf")
                fileType = "application/pdf";
            else if (fileExtension == ".png")
                fileType = "image/png";
            else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                fileType = "image/jpeg";
            else if (fileExtension == ".gif")
                fileType = "image/gif";
            else if (fileExtension == ".bmp")
                fileType = "image/bmp";
            else
                fileType = "image/webp";

            filePath = await _fileService.SaveFileAsync(createPrescriptionDto.File, "prescriptions");
        }

        var createPrescriptionDtoInternal = new CreatePrescriptionDto
        {
            MedicalRecordId = createPrescriptionDto.MedicalRecordId,
            Title = createPrescriptionDto.Title,
            Status = createPrescriptionDto.Status
        };

        var prescriptionService = _prescriptionService as PrescriptionService;
        var result = prescriptionService != null
            ? await prescriptionService.Add(createPrescriptionDtoInternal, filePath, fileType)
            : await _prescriptionService.Add(createPrescriptionDtoInternal);

        return result.ErrorType switch
        {
            ServiceErrorType.Success => CreatedAtAction(nameof(GetById), new { id = result.Data!.PrescriptionId }, result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet("medical-record/{medicalRecordId}")]
    [ProducesResponseType(typeof(IEnumerable<PrescriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetByMedicalRecordId(Guid medicalRecordId)
    {
        var result = await _prescriptionService.GetByMedicalRecordId(medicalRecordId);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(typeof(IEnumerable<PrescriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetByPatientId(string patientId)
    {
        var result = await _prescriptionService.GetByPatientId(patientId, GetDoctorIdForScope());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanEditPrescription)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrescriptionDto>> Update([FromForm] UpdatePrescriptionWithFileDto updatePrescriptionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string? filePath = null;
        string? fileType = null;

        if (updatePrescriptionDto.File != null && updatePrescriptionDto.File.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(updatePrescriptionDto.File.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Only PDF and image files are allowed.");

            if (updatePrescriptionDto.File.Length > 10 * 1024 * 1024)
                return BadRequest("File size exceeds maximum allowed size of 10MB");

            if (fileExtension == ".pdf")
                fileType = "application/pdf";
            else if (fileExtension == ".png")
                fileType = "image/png";
            else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                fileType = "image/jpeg";
            else if (fileExtension == ".gif")
                fileType = "image/gif";
            else if (fileExtension == ".bmp")
                fileType = "image/bmp";
            else
                fileType = "image/webp";

            filePath = await _fileService.SaveFileAsync(updatePrescriptionDto.File, "prescriptions");
        }

        var existingPrescription = await _prescriptionService.GetById(updatePrescriptionDto.PrescriptionId, GetDoctorIdForScope());
        if (existingPrescription.ErrorType == ServiceErrorType.NotFound)
            return NotFound("Prescription not found");

        var prescriptionDto = new PrescriptionDto
        {
            PrescriptionId = updatePrescriptionDto.PrescriptionId,
            Title = updatePrescriptionDto.Title,
            Status = updatePrescriptionDto.Status,
            MedicalRecordId = existingPrescription.Data!.MedicalRecordId
        };

        var prescriptionService = _prescriptionService as PrescriptionService;
        var result = prescriptionService != null
            ? await prescriptionService.Update(prescriptionDto, filePath, fileType)
            : await _prescriptionService.Update(prescriptionDto);
        
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PrescriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<PrescriptionDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _prescriptionService.GetAll(GetDoctorIdForScope(), null, pageNumber, pageSize);
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

    [Authorize(Policy = AuthorizationPolicies.CanViewPrescriptions)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrescriptionDto>> GetById(Guid id)
    {
        var result = await _prescriptionService.GetById(id, GetDoctorIdForScope());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanDeletePrescription)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _prescriptionService.Delete(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
