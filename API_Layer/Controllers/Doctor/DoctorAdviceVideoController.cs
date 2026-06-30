using System.Security.Claims;
using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using ClinicAPI.DTOs;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DoctorAdviceVideoController : ControllerBase
{
    private readonly IDoctorAdviceVideoService _adviceVideoService;
    private readonly IFileService _fileService;
    private readonly IValidator<CreateDoctorAdviceVideoDto> _createValidator;
    private readonly IValidator<UpdateDoctorAdviceVideoDto> _updateValidator;

    private const long MaxVideoSizeBytes = FileService.DefaultMaxVideoSizeBytes; // 100MB
    private const string VideoSubfolder = "videos";

    public DoctorAdviceVideoController(
        IDoctorAdviceVideoService adviceVideoService,
        IFileService fileService,
        IValidator<CreateDoctorAdviceVideoDto> createValidator,
        IValidator<UpdateDoctorAdviceVideoDto> updateValidator)
    {
        _adviceVideoService = adviceVideoService;
        _fileService = fileService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    private string? GetCurrentDoctorId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return null;
        if (User.IsInRole(Roles.SuperAdmin)) return userId;
        if (User.IsInRole(Roles.Doctor)) return userId;
        var roleValue = User.FindFirst("role")?.Value;
        if (string.Equals(roleValue, Roles.Doctor, StringComparison.OrdinalIgnoreCase)) return userId;
        return null;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<DoctorAdviceVideoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<DoctorAdviceVideoDto>>> GetAllPublished(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adviceVideoService.GetAllPublishedAsync(pageNumber, pageSize);
        if (!result.IsSuccess)
            return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Doctor)]
    [ProducesResponseType(typeof(IEnumerable<DoctorAdviceVideoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DoctorAdviceVideoDto>>> GetMyVideos()
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId))
            return Forbid();
        var result = await _adviceVideoService.GetByDoctorIdAsync(doctorId);
        if (!result.IsSuccess)
            return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DoctorAdviceVideoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorAdviceVideoDto>> GetById(Guid id)
    {
        var doctorId = GetCurrentDoctorId();
        var result = await _adviceVideoService.GetByIdAsync(id, doctorId);
        if (!result.IsSuccess)
            return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Doctor)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DoctorAdviceVideoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorAdviceVideoDto>> Create([FromForm] CreateDoctorAdviceVideoWithFileDto dto)
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId))
            return Forbid();

        if (dto.Video == null || dto.Video.Length == 0)
            return BadRequest("Video file is required.");
        if (!_fileService.IsValidVideo(dto.Video))
            return BadRequest("Invalid video type. Allowed: .mp4, .webm, .mov, .avi, .mkv");
        if (!_fileService.IsValidFileSize(dto.Video, MaxVideoSizeBytes))
            return BadRequest($"Video size must not exceed {MaxVideoSizeBytes / (1024 * 1024)}MB.");

        string videoUrl;
        try
        {
            videoUrl = await _fileService.SaveFileAsync(dto.Video, VideoSubfolder);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to save video: {ex.Message}");
        }

        var createDto = new CreateDoctorAdviceVideoDto
        {
            Title = dto.Title,
            Description = dto.Description,
            IsPublished = dto.IsPublished,
            VideoUrl = videoUrl
        };
        var validationResult = await _createValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
            return BadRequest(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var result = await _adviceVideoService.CreateAsync(createDto, doctorId);
        if (!result.IsSuccess)
            return ResultToActionResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DoctorAdviceVideoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorAdviceVideoDto>> Update(Guid id, [FromForm] UpdateDoctorAdviceVideoWithFileDto dto)
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId))
            return Forbid();

        var updateDto = new UpdateDoctorAdviceVideoDto
        {
            Title = dto.Title,
            Description = dto.Description,
            IsPublished = dto.IsPublished,
            VideoUrl = null
        };
        if (dto.Video != null && dto.Video.Length > 0)
        {
            if (!_fileService.IsValidVideo(dto.Video))
                return BadRequest("Invalid video type. Allowed: .mp4, .webm, .mov, .avi, .mkv");
            if (!_fileService.IsValidFileSize(dto.Video, MaxVideoSizeBytes))
                return BadRequest($"Video size must not exceed {MaxVideoSizeBytes / (1024 * 1024)}MB.");
            try
            {
                updateDto.VideoUrl = await _fileService.SaveFileAsync(dto.Video, VideoSubfolder);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to save video: {ex.Message}");
            }
        }
        var validationResult = await _updateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
            return BadRequest(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var result = await _adviceVideoService.UpdateAsync(id, updateDto, doctorId);
        if (!result.IsSuccess)
            return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId))
            return Forbid();
        var result = await _adviceVideoService.DeleteAsync(id, doctorId);
        if (!result.IsSuccess)
            return ResultToActionResult(result);
        return NoContent();
    }

    private ActionResult ResultToActionResult<T>(Result<T> result)
    {
        return result.ErrorType switch
        {
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.Conflict => Conflict(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }
}
