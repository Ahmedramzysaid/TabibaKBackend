using System.Text.Json;
using BusinessLayer.Interfaces;
using ClinicAPI.DTOs;
using ClinicAPI.Services;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]

public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IFileService _fileService;
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDoctorVerificationService _verificationService;
    private readonly IEmailService _emailService;

    public DoctorController(IDoctorService doctorService, IFileService fileService, IAuthService authService, UserManager<ApplicationUser> userManager, IDoctorVerificationService verificationService, IEmailService emailService)
    {
        _doctorService = doctorService;
        _fileService = fileService;
        _authService = authService;
        _userManager = userManager;
        _verificationService = verificationService;
        _emailService = emailService;
    }

    [AllowAnonymous]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DoctorCreateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DoctorCreateResponseDto>> Add([FromForm] CreateDoctorDto createDoctorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var verificationResult = await _verificationService.VerifyDoctorNameAsync(createDoctorDto.FullName);
        if (!verificationResult.IsVerified)
            return BadRequest("You are not doctor");

        static IFormFile? FirstFile(IFormFileCollection files, string name)
        {
            var list = files.GetFiles(name);
            return list?.FirstOrDefault(f => f != null && f.Length > 0 && !string.IsNullOrWhiteSpace(f.FileName));
        }
        var idFrontFile = FirstFile(Request.Form.Files, "IdImageFront");
        var idBackFile = FirstFile(Request.Form.Files, "IdImageBack");
        var profileFile = FirstFile(Request.Form.Files, "ProfileImage");

        if (idFrontFile == null)
            return BadRequest("ID front image is required");
        if (idBackFile == null)
            return BadRequest("ID back image is required");

        if (!_fileService.IsValidImage(idFrontFile))
            return BadRequest("Invalid image format for ID front. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");
        if (!_fileService.IsValidImage(idBackFile))
            return BadRequest("Invalid image format for ID back. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");
        if (!_fileService.IsValidFileSize(idFrontFile))
            return BadRequest("ID front image file size exceeds maximum allowed size of 5MB");
        if (!_fileService.IsValidFileSize(idBackFile))
            return BadRequest("ID back image file size exceeds maximum allowed size of 5MB");

        if (profileFile != null)
        {
            if (!_fileService.IsValidImage(profileFile))
                return BadRequest("Invalid image format for profile image. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");
            if (!_fileService.IsValidFileSize(profileFile))
                return BadRequest("Profile image file size exceeds maximum allowed size of 5MB");
        }

        string? idImageFrontUrl = null;
        string? idImageBackUrl = null;
        string? profileImageUrl = null;

        try
        {
            idImageFrontUrl = await _fileService.SaveFileAsync(idFrontFile, "doctors");
            idImageBackUrl = await _fileService.SaveFileAsync(idBackFile, "doctors");
            if (profileFile != null)
                profileImageUrl = await _fileService.SaveFileAsync(profileFile, "doctors");

            DateTime? parsedDateOfBirth = null;
            if (!string.IsNullOrWhiteSpace(createDoctorDto.DateOfBirth))
            {
                if (!DateTime.TryParseExact(createDoctorDto.DateOfBirth, "MM-dd-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    return BadRequest("Invalid date format. Date of birth must be in MM-dd-yyyy format (e.g., 01-15-1990)");
                parsedDateOfBirth = parsedDate;
            }

            var normalizedEmail = createDoctorDto.Email.Trim();

            var existingNameUser = await _userManager.Users.FirstOrDefaultAsync(u => u.FullName == createDoctorDto.FullName);
            if (existingNameUser != null)
                return BadRequest("User with this name already exists. Please use a different name.");

            var existingEmailUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingEmailUser != null)
                return BadRequest("User with this email already exists. Please use a different email.");

            var doctorDto = new DoctorDto
            {
                Id = string.Empty,
                FullName = createDoctorDto.FullName,
                Email = normalizedEmail,
                DateOfBirth = parsedDateOfBirth,
                Gender = createDoctorDto.Gender,
                Latitude = createDoctorDto.Latitude,
                Longitude = createDoctorDto.Longitude,
                Specialization = verificationResult.Specialization ?? createDoctorDto.Specialization,
                IdNo = createDoctorDto.IdNo,
                IdImageFrontUrl = idImageFrontUrl,
                IdImageBackUrl = idImageBackUrl,
                ProfileImageUrl = profileImageUrl,
                DateOfRegistration = DateTime.UtcNow
            };

            var pendingJson = JsonSerializer.Serialize(new { DoctorDto = doctorDto, Password = createDoctorDto.Password });
            await _authService.CachePendingRegistrationAsync(normalizedEmail, pendingJson);

            return Ok(new 
            { 
                message = "Verification code sent to your email. Please verify within 60 seconds.",
                email = normalizedEmail
            });
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(idImageFrontUrl))
            {
                try { await _fileService.DeleteFileAsync(idImageFrontUrl); } catch { }
            }
            if (!string.IsNullOrEmpty(idImageBackUrl))
            {
                try { await _fileService.DeleteFileAsync(idImageBackUrl); } catch { }
            }
            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                try { await _fileService.DeleteFileAsync(profileImageUrl); } catch { }
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while saving images: {ex.Message}");
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(DoctorCreateResponseDto), StatusCodes.Status201Created)]
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
        var doctorDto = JsonSerializer.Deserialize<DoctorDto>(root.GetProperty("DoctorDto").GetRawText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var password = root.GetProperty("Password").GetString();

        if (doctorDto == null || string.IsNullOrEmpty(password))
            return BadRequest(new { message = "Registration data is invalid. Please register again." });

        var result = await _doctorService.Add(doctorDto, password);
        if (result.ErrorType != ServiceErrorType.Success || result.Data == null)
        {
            return result.ErrorType switch
            {
                ServiceErrorType.ValidationError => BadRequest(result.Message),
                ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
                _ => StatusCode(StatusCodes.Status500InternalServerError,
                    result.Message ?? "An error occurred while creating the doctor.")
            };
        }

        var user = await _userManager.FindByIdAsync(result.Data.Id);
        if (user != null)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        var (token, refreshToken, refreshTokenExpiresOn) = await _authService.GenerateTokensForUser(user!);
        var response = new DoctorCreateResponseDto
        {
            Doctor = result.Data,
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiresOn = refreshTokenExpiresOn,
            IsAuthenticated = true
        };

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, response);
    }

    [Authorize]
    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> Update([FromForm] UpdateDoctorDto updateDoctorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(updateDoctorDto.Id))
            return BadRequest("Doctor Id is required for update.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var canEditAllDoctors = User.HasClaim(ClaimConstants.Permission, ClaimConstants.EditDoctor);
        var isUpdatingOwnData = currentUserId != null && currentUserId == updateDoctorDto.Id;

        if (!isUpdatingOwnData && !canEditAllDoctors)
            return Forbid("You do not have permission to update this doctor's data");

        var existingDoctorResult = await _doctorService.GetById(updateDoctorDto.Id);
        if (existingDoctorResult.ErrorType != ServiceErrorType.Success || existingDoctorResult.Data == null)
            return NotFound("Doctor is not found to update");

        var existingDoctor = existingDoctorResult.Data;
        string? idImageFrontUrl = existingDoctor.IdImageFrontUrl;
        string? idImageBackUrl = existingDoctor.IdImageBackUrl;
        string? profileImageUrl = existingDoctor.ProfileImageUrl;

        string? newIdImageFrontUrl = null;
        string? newIdImageBackUrl = null;
        string? newProfileImageUrl = null;

        try
        {
            if (updateDoctorDto.IdImageFront != null && updateDoctorDto.IdImageFront.Length > 0)
            {
                if (!_fileService.IsValidImage(updateDoctorDto.IdImageFront))
                    return BadRequest("Invalid image format for ID front. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");
                if (!_fileService.IsValidFileSize(updateDoctorDto.IdImageFront))
                    return BadRequest("ID front image file size exceeds maximum allowed size of 5MB");
                newIdImageFrontUrl = await _fileService.SaveFileAsync(updateDoctorDto.IdImageFront, "doctors");
                idImageFrontUrl = newIdImageFrontUrl;
            }

            if (updateDoctorDto.IdImageBack != null && updateDoctorDto.IdImageBack.Length > 0)
            {
                if (!_fileService.IsValidImage(updateDoctorDto.IdImageBack))
                    return BadRequest("Invalid image format for ID back. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");
                if (!_fileService.IsValidFileSize(updateDoctorDto.IdImageBack))
                    return BadRequest("ID back image file size exceeds maximum allowed size of 5MB");
                newIdImageBackUrl = await _fileService.SaveFileAsync(updateDoctorDto.IdImageBack, "doctors");
                idImageBackUrl = newIdImageBackUrl;
            }

            if (updateDoctorDto.ProfileImage != null && updateDoctorDto.ProfileImage.Length > 0)
            {
                if (!_fileService.IsValidImage(updateDoctorDto.ProfileImage))
                    return BadRequest("Invalid image format for profile image. Only .jpg, .jpeg, .png, .gif, .bmp, .webp are allowed");
                if (!_fileService.IsValidFileSize(updateDoctorDto.ProfileImage))
                    return BadRequest("Profile image file size exceeds maximum allowed size of 5MB");
                newProfileImageUrl = await _fileService.SaveFileAsync(updateDoctorDto.ProfileImage, "doctors");
                profileImageUrl = newProfileImageUrl;
            }

            DateTime? parsedDateOfBirth = null;
            if (!string.IsNullOrWhiteSpace(updateDoctorDto.DateOfBirth))
            {
                if (!DateTime.TryParseExact(updateDoctorDto.DateOfBirth, "MM-dd-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    return BadRequest("Invalid date format. Date of birth must be in MM-dd-yyyy format (e.g., 01-15-1990)");
                parsedDateOfBirth = parsedDate;
            }

            bool IsPlaceholderValue(string? value) => 
                string.IsNullOrWhiteSpace(value) || 
                value.Equals("string", StringComparison.OrdinalIgnoreCase);

            var doctorDto = new DoctorDto
            {
                Id = updateDoctorDto.Id,
                FullName = IsPlaceholderValue(updateDoctorDto.FullName) ? null : updateDoctorDto.FullName,
                Email = IsPlaceholderValue(updateDoctorDto.Email) ? null : updateDoctorDto.Email,
                DateOfBirth = parsedDateOfBirth,
                Gender = IsPlaceholderValue(updateDoctorDto.Gender) ? null : updateDoctorDto.Gender,
                Latitude = updateDoctorDto.Latitude.HasValue && updateDoctorDto.Latitude.Value != 0 ? updateDoctorDto.Latitude : null,
                Longitude = updateDoctorDto.Longitude.HasValue && updateDoctorDto.Longitude.Value != 0 ? updateDoctorDto.Longitude : null,
                Specialization = IsPlaceholderValue(updateDoctorDto.Specialization) ? null : updateDoctorDto.Specialization,
                IdNo = IsPlaceholderValue(updateDoctorDto.IdNo) ? null : updateDoctorDto.IdNo,
                Price = updateDoctorDto.Price,
                IsAvailable = updateDoctorDto.IsAvailable ?? existingDoctor.IsAvailable,
                IdImageFrontUrl = idImageFrontUrl,
                IdImageBackUrl = idImageBackUrl,
                ProfileImageUrl = profileImageUrl,
                DateOfRegistration = existingDoctor.DateOfRegistration,
            };

            var result = await _doctorService.Update(doctorDto);

            if (result.ErrorType == ServiceErrorType.Success)
            {
                if (!string.IsNullOrEmpty(newIdImageFrontUrl) && !string.IsNullOrEmpty(existingDoctor.IdImageFrontUrl))
                {
                    try { await _fileService.DeleteFileAsync(existingDoctor.IdImageFrontUrl); } catch { }
                }
                if (!string.IsNullOrEmpty(newIdImageBackUrl) && !string.IsNullOrEmpty(existingDoctor.IdImageBackUrl))
                {
                    try { await _fileService.DeleteFileAsync(existingDoctor.IdImageBackUrl); } catch { }
                }
                if (!string.IsNullOrEmpty(newProfileImageUrl) && !string.IsNullOrEmpty(existingDoctor.ProfileImageUrl))
                {
                    try { await _fileService.DeleteFileAsync(existingDoctor.ProfileImageUrl); } catch { }
                }
                return NoContent();
            }
            else
            {
                if (!string.IsNullOrEmpty(newIdImageFrontUrl))
                {
                    try { await _fileService.DeleteFileAsync(newIdImageFrontUrl); } catch { }
                }
                if (!string.IsNullOrEmpty(newIdImageBackUrl))
                {
                    try { await _fileService.DeleteFileAsync(newIdImageBackUrl); } catch { }
                }
                if (!string.IsNullOrEmpty(newProfileImageUrl))
                {
                    try { await _fileService.DeleteFileAsync(newProfileImageUrl); } catch { }
                }

                return result.ErrorType switch
                {
                    ServiceErrorType.ValidationError => BadRequest(result.Message),
                    ServiceErrorType.NotFound => NotFound(result.Message),
                    ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        result.Message ?? "An error occurred while processing your request.")
                };
            }
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(newIdImageFrontUrl))
            {
                try { await _fileService.DeleteFileAsync(newIdImageFrontUrl); } catch { }
            }
            if (!string.IsNullOrEmpty(newIdImageBackUrl))
            {
                try { await _fileService.DeleteFileAsync(newIdImageBackUrl); } catch { }
            }
            if (!string.IsNullOrEmpty(newProfileImageUrl))
            {
                try { await _fileService.DeleteFileAsync(newProfileImageUrl); } catch { }
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while updating doctor: {ex.Message}");
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> GetById([FromRoute] string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var canViewAllDoctors = User.HasClaim(ClaimConstants.Permission, ClaimConstants.ViewDoctors);
        var isViewingOwnData = currentUserId != null && currentUserId == id;

        if (!isViewingOwnData && !canViewAllDoctors)
            return Forbid("You do not have permission to view this doctor's data");

        var result = await _doctorService.GetById(id);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(new { message = result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while processing your request." })
        };
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<DoctorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _doctorService.GetAll(pageNumber, pageSize);
            return result.ErrorType switch
            {
                ServiceErrorType.Success => Ok(result.Data),
                ServiceErrorType.NotFound => Ok(PaginatedResult<DoctorDto>.Create(
                    Enumerable.Empty<DoctorDto>(), 0, pageNumber, pageSize)),
                _ => StatusCode(StatusCodes.Status500InternalServerError,
                    new { detail = result.Message ?? "An error occurred." })
            };
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException != null ? $"{ex.Message} | {ex.InnerException.Message}" : ex.Message;
            return StatusCode(StatusCodes.Status500InternalServerError, new { detail });
        }
    }

    [Authorize(Policy = AuthorizationPolicies.CanDeleteDoctor)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var getDoctorResult = await _doctorService.GetById(id);
        if (getDoctorResult.ErrorType != ServiceErrorType.Success || getDoctorResult.Data == null)
            return NotFound("Doctor not found");

        var doctor = getDoctorResult.Data;
        var imageUrls = new List<string>();

        if (!string.IsNullOrEmpty(doctor.IdImageFrontUrl))
            imageUrls.Add(doctor.IdImageFrontUrl);
        if (!string.IsNullOrEmpty(doctor.IdImageBackUrl))
            imageUrls.Add(doctor.IdImageBackUrl);
        if (!string.IsNullOrEmpty(doctor.ProfileImageUrl))
            imageUrls.Add(doctor.ProfileImageUrl);

        var result = await _doctorService.Delete(id);

        if (result.ErrorType == ServiceErrorType.Success)
        {
            foreach (var imageUrl in imageUrls)
            {
                try { await _fileService.DeleteFileAsync(imageUrl); } catch { }
            }
            return Ok(new { message = "Doctor and associated user account deleted successfully" });
        }

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request.")
        };
    }

    [AllowAnonymous]
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<DoctorSearchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DoctorSearchDto>>> SearchDoctors(
        [FromQuery] string? specialization = null,
        [FromQuery] string? name = null)
    {
        var result = await _doctorService.SearchDoctors(specialization, name);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An error occurred")
        };
    }

    [AllowAnonymous]
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(IEnumerable<NearbyDoctorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<NearbyDoctorDto>>> GetNearby(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] string specialization,
        [FromQuery] string sortBy = "score")
    {
        const double maxDistanceKm = 1000;
        var result = await _doctorService.GetNearbyBySpecialization(latitude, longitude, specialization, maxDistanceKm, sortBy);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(result.Data),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An error occurred")
        };
    }
}
