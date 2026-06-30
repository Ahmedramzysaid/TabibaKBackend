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
public class TestingController : ControllerBase
{
    private readonly ITestingService _testingService;
    private readonly IFileService _fileService;

    public TestingController(ITestingService testingService, IFileService fileService)
    {
        _testingService = testingService;
        _fileService = fileService;
    }

    private string? GetDoctorIdForScope() => User.IsInRole(Roles.Doctor) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    [Authorize(Policy = AuthorizationPolicies.CanViewMedicalRecords)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TestingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<TestingDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _testingService.GetAll(GetDoctorIdForScope(), pageNumber, pageSize);
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
    [ProducesResponseType(typeof(TestingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestingDto>> GetById(Guid id)
    {
        var result = await _testingService.GetById(id, GetDoctorIdForScope());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewMedicalRecords)]
    [HttpGet("medical-record/{medicalRecordId}")]
    [ProducesResponseType(typeof(IEnumerable<TestingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TestingDto>>> GetByMedicalRecordId(Guid medicalRecordId)
    {
        var result = await _testingService.GetByMedicalRecordId(medicalRecordId);
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
    [ProducesResponseType(typeof(IEnumerable<TestingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TestingDto>>> GetByPatientId(string patientId)
    {
        var result = await _testingService.GetByPatientId(patientId, GetDoctorIdForScope());
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanCreateMedicalRecord)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(TestingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestingDto>> Add([FromForm] CreateTestingWithFileDto createTestingDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string? filePath = null;
        string? fileType = null;

        if (createTestingDto.File != null && createTestingDto.File.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(createTestingDto.File.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Only PDF and image files are allowed.");

            if (createTestingDto.File.Length > 10 * 1024 * 1024)
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

            filePath = await _fileService.SaveFileAsync(createTestingDto.File, "testings");
        }

        var createTestingDtoInternal = new CreateTestingDto
        {
            MedicalRecordId = createTestingDto.MedicalRecordId,
            Title = createTestingDto.Title,
            DateExam = createTestingDto.DateExam
        };

        var testingService = _testingService as TestingService;
        var result = testingService != null 
            ? await testingService.Add(createTestingDtoInternal, filePath, fileType)
            : await _testingService.Add(createTestingDtoInternal);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => CreatedAtAction(nameof(GetById), new { id = result.Data!.TestingId }, result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanEditMedicalRecord)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(TestingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestingDto>> Update([FromForm] UpdateTestingWithFileDto updateTestingDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string? filePath = null;
        string? fileType = null;

        if (updateTestingDto.File != null && updateTestingDto.File.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(updateTestingDto.File.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Only PDF and image files are allowed.");

            if (updateTestingDto.File.Length > 10 * 1024 * 1024)
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

            filePath = await _fileService.SaveFileAsync(updateTestingDto.File, "testings");
        }

        var existingTesting = await _testingService.GetById(updateTestingDto.TestingId, GetDoctorIdForScope());
        if (existingTesting.ErrorType == ServiceErrorType.NotFound)
            return NotFound("Testing not found");

        var testingDto = new TestingDto
        {
            TestingId = updateTestingDto.TestingId,
            Title = updateTestingDto.Title,
            DateExam = updateTestingDto.DateExam,
            MedicalRecordId = existingTesting.Data!.MedicalRecordId // Get from existing record
        };

        var testingService = _testingService as TestingService;
        var result = testingService != null 
            ? await testingService.Update(testingDto, filePath, fileType)
            : await _testingService.Update(testingDto);
        
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
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
        var result = await _testingService.Delete(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
