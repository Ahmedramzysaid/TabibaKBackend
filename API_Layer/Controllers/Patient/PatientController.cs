using System.Text.Json;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Interfaces.ServicesInterfaces;
using DomainLayer.Models;
using ClinicAPI.DTOs;
using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IFileService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public PatientController(IPatientService patientService, IFileService fileService, UserManager<ApplicationUser> userManager, IAuthService authService, IEmailService emailService)
    {
        _patientService = patientService;
        _fileService = fileService;
        _userManager = userManager;
        _authService = authService;
        _emailService = emailService;
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPatients)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<PatientDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _patientService.GetAll(pageNumber, pageSize);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => Ok(PaginatedResult<PatientDto>.Create(
                Enumerable.Empty<PatientDto>(), 0, pageNumber, pageSize)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }

    [AllowAnonymous]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PatientCreateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientDto>> Add([FromForm] CreatePatientDto createPatientDto)
    {
        string? profileImageUrl = null;
        if (createPatientDto.ProfileImage != null && createPatientDto.ProfileImage.Length > 0)
        {
            if (!_fileService.IsValidImage(createPatientDto.ProfileImage))
                return BadRequest("Invalid image format for profile image. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");

            if (!_fileService.IsValidFileSize(createPatientDto.ProfileImage))
                return BadRequest("Profile image file size exceeds maximum allowed size of 5MB");

            try
            {
                profileImageUrl = await _fileService.SaveFileAsync(createPatientDto.ProfileImage, "patients");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while saving profile image: {ex.Message}");
            }
        }

        try
        {
            DateTime? parsedDateOfBirth = null;
            if (!string.IsNullOrWhiteSpace(createPatientDto.DateOfBirth))
            {
                if (!DateTime.TryParseExact(createPatientDto.DateOfBirth, "MM-dd-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    return BadRequest("Invalid date format. Date of birth must be in MM-dd-yyyy format (e.g., 01-15-1990)");
                parsedDateOfBirth = parsedDate;
            }

            var normalizedEmail = createPatientDto.Email.Trim();
            var existingEmailUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingEmailUser != null)
                return BadRequest("User with this email already exists. Please use a different email.");

            var patientDto = new PatientDto
            {
                Id = string.Empty,
                FullName = createPatientDto.FullName,
                Email = normalizedEmail,
                DateOfBirth = parsedDateOfBirth,
                Gender = createPatientDto.Gender,
                Latitude = createPatientDto.Latitude,
                Longitude = createPatientDto.Longitude,
                ProfileImageUrl = profileImageUrl,
                DateOfRegistration = DateTime.UtcNow
            };

            var pendingJson = JsonSerializer.Serialize(new { PatientDto = patientDto, Password = createPatientDto.Password });
            await _authService.CachePendingRegistrationAsync(normalizedEmail, pendingJson);

            return Ok(new 
            { 
                message = "Verification code sent to your email. Please verify within 60 seconds.",
                email = normalizedEmail
            });
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                try { await _fileService.DeleteFileAsync(profileImageUrl); } catch { }
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while processing your request: {ex.Message}");
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(PatientCreateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailOtpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, json) = await _authService.VerifyOtpAndGetPendingAsync(dto);
        if (!success || json == null)
            return BadRequest(new { message });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var patientDto = JsonSerializer.Deserialize<PatientDto>(root.GetProperty("PatientDto").GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var password = root.GetProperty("Password").GetString();

        if (patientDto == null || string.IsNullOrEmpty(password))
            return BadRequest(new { message = "Registration data is invalid. Please register again." });

        var result = await _patientService.Add(patientDto, password);
        if (result.ErrorType != ServiceErrorType.Success || result.Data == null)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.ValidationError => BadRequest(result.Message),
                ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
                _ => StatusCode(StatusCodes.Status500InternalServerError,
                    result.Message ?? "An error occurred while creating the patient.")
            };
        }

        var user = await _userManager.FindByIdAsync(result.Data.Id);
        if (user != null)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        var (token, refreshToken, refreshTokenExpiresOn) = await _authService.GenerateTokensForUser(user!);
        var response = new PatientCreateResponseDto
        {
            Patient = result.Data,
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiresOn = refreshTokenExpiresOn,
            IsAuthenticated = true
        };

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, response);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDto>> GetById([FromRoute] string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var canViewAllPatients = User.HasClaim(ClaimConstants.Permission, ClaimConstants.ViewPatients);
        var isViewingOwnData = currentUserId != null && currentUserId == id;

        if (!isViewingOwnData && !canViewAllPatients)
            return Forbid("You do not have permission to view this patient's data");

        var result = await _patientService.GetById(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanDeletePatient)]
    [HttpDelete("{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string patientId)
    {
        var getPatientResult = await _patientService.GetById(patientId);
        if (getPatientResult.ErrorType != ServiceErrorType.Success || getPatientResult.Data == null)
            return NotFound(getPatientResult.Message ?? "Patient not found");

        var patient = getPatientResult.Data;
        var profileImageUrl = patient.ProfileImageUrl;

        var result = await _patientService.Delete(patientId);

        if (result.ErrorType == ServiceErrorType.Success)
        {
            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                try { await _fileService.DeleteFileAsync(profileImageUrl); } catch { }
            }
            return Ok(new { message = "Patient and associated user account deleted successfully" });
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message ?? "An unexpected error occurred")
        };
    }

    [Authorize]
    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromForm] UpdatePatientDto updatePatientDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var canEditAllPatients = User.HasClaim(ClaimConstants.Permission, ClaimConstants.EditPatient);
        var isUpdatingOwnData = currentUserId != null && currentUserId == updatePatientDto.Id;

        if (!isUpdatingOwnData && !canEditAllPatients)
            return Forbid("You do not have permission to update this patient's data");

        var existingPatientResult = await _patientService.GetById(updatePatientDto.Id);
        if (existingPatientResult.ErrorType != ServiceErrorType.Success || existingPatientResult.Data == null)
            return NotFound("Patient is not found to update");

        var existingPatient = existingPatientResult.Data;
        string? newProfileImageUrl = null;

        try
        {
            if (updatePatientDto.ProfileImage != null && updatePatientDto.ProfileImage.Length > 0)
                newProfileImageUrl = await _fileService.SaveFileAsync(updatePatientDto.ProfileImage, "patients");

            DateTime? parsedDateOfBirth = null;
            if (!string.IsNullOrWhiteSpace(updatePatientDto.DateOfBirth))
            {
                if (!DateTime.TryParseExact(updatePatientDto.DateOfBirth, "MM-dd-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    return BadRequest("Invalid date format. Date of birth must be in MM-dd-yyyy format (e.g., 01-15-1990)");
                parsedDateOfBirth = parsedDate;
            }

            bool IsPlaceholderValue(string? value) => 
                string.IsNullOrWhiteSpace(value) || 
                value.Equals("string", StringComparison.OrdinalIgnoreCase);

            var patientDto = new PatientDto
            {
                Id = updatePatientDto.Id,
                FullName = IsPlaceholderValue(updatePatientDto.FullName) ? existingPatient.FullName : updatePatientDto.FullName!,
                Email = IsPlaceholderValue(updatePatientDto.Email) ? existingPatient.Email : updatePatientDto.Email,
                DateOfBirth = parsedDateOfBirth ?? existingPatient.DateOfBirth,
                Gender = IsPlaceholderValue(updatePatientDto.Gender) ? existingPatient.Gender : updatePatientDto.Gender!,
                Latitude = (updatePatientDto.Latitude.HasValue && updatePatientDto.Latitude.Value != 0) ? updatePatientDto.Latitude : existingPatient.Latitude,
                Longitude = (updatePatientDto.Longitude.HasValue && updatePatientDto.Longitude.Value != 0) ? updatePatientDto.Longitude : existingPatient.Longitude,
                ProfileImageUrl = newProfileImageUrl ?? existingPatient.ProfileImageUrl,
                DateOfRegistration = existingPatient.DateOfRegistration
            };

            var result = await _patientService.Update(patientDto);

            if (result.ErrorType == ServiceErrorType.Success)
            {
                if (!string.IsNullOrEmpty(newProfileImageUrl) && 
                    !string.IsNullOrEmpty(existingPatient.ProfileImageUrl) &&
                    existingPatient.ProfileImageUrl != newProfileImageUrl)
                {
                    try { await _fileService.DeleteFileAsync(existingPatient.ProfileImageUrl); } catch { }
                }
                return NoContent();
            }
            else
            {
                if (!string.IsNullOrEmpty(newProfileImageUrl))
                {
                    try { await _fileService.DeleteFileAsync(newProfileImageUrl); } catch { }
                }

                return result.ErrorType switch
                {
                    ServiceErrorType.ValidationError => BadRequest(result.Message),
                    ServiceErrorType.NotFound => NotFound(result.Message),
                    ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status500InternalServerError, result.Message),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
                };
            }
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(newProfileImageUrl))
            {
                try { await _fileService.DeleteFileAsync(newProfileImageUrl); } catch { }
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while processing your request: {ex.Message}");
        }
    }
}
