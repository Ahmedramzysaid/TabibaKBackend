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
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DoctorAdvicePostController : ControllerBase
{
    private readonly IDoctorAdvicePostService _postService;

    public DoctorAdvicePostController(IDoctorAdvicePostService postService)
    {
        _postService = postService;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    }

    private string? GetCurrentDoctorId()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return null;
        if (User.IsInRole(Roles.SuperAdmin)) return userId;
        if (User.IsInRole(Roles.Doctor)) return userId;
        var roleValue = User.FindFirst("role")?.Value;
        if (string.Equals(roleValue, Roles.Doctor, StringComparison.OrdinalIgnoreCase)) return userId;
        return null;
    }


    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<DoctorAdvicePostDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<DoctorAdvicePostDto>>> GetAllPublished(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var result = await _postService.GetAllPublishedAsync(userId, pageNumber, pageSize);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Doctor)]
    [ProducesResponseType(typeof(IEnumerable<DoctorAdvicePostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DoctorAdvicePostDto>>> GetMyPosts()
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId)) return Forbid();
        var result = await _postService.GetMyPostsAsync(doctorId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DoctorAdvicePostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorAdvicePostDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _postService.GetByIdAsync(id, userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Doctor)]
    [ProducesResponseType(typeof(DoctorAdvicePostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorAdvicePostDto>> Create([FromBody] CreateDoctorAdvicePostDto dto)
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId)) return Forbid();

        var result = await _postService.CreateAsync(dto, doctorId, null);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Doctor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var doctorId = GetCurrentDoctorId();
        if (string.IsNullOrEmpty(doctorId)) return Forbid();
        var result = await _postService.DeleteAsync(id, doctorId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return NoContent();
    }


    [HttpPost("{id:guid}/like")]
    [Authorize(Policy = AuthorizationPolicies.CanViewDoctorAdviceVideos)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleLike(Guid id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _postService.ToggleLikeAsync(id, userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(new { IsLiked = result.Data });
    }

    [HttpGet("{id:guid}/likes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<AdviceLikeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AdviceLikeDto>>> GetLikes(Guid id)
    {
        var result = await _postService.GetLikesAsync(id);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }


    [HttpPost("{id:guid}/save/{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleSavePost(Guid id, string userId)
    {
        if (string.IsNullOrEmpty(userId)) return BadRequest("User ID is required");
        var result = await _postService.ToggleSavePostAsync(id, userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(new { IsSaved = result.Data });
    }

    [HttpGet("saved/{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SavedPostDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SavedPostDto>>> GetSavedPosts(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return BadRequest("User ID is required");
        var result = await _postService.GetSavedPostsAsync(userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }


    [HttpPost("{id:guid}/comments")]
    [Authorize(Policy = AuthorizationPolicies.CanViewDoctorAdviceVideos)]
    [ProducesResponseType(typeof(AdviceCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdviceCommentDto>> AddComment(Guid id, [FromBody] CreateAdviceCommentDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _postService.AddCommentAsync(id, dto, userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Created(string.Empty, result.Data);
    }

    [HttpGet("{id:guid}/comments")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<AdviceCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AdviceCommentDto>>> GetComments(Guid id)
    {
        var result = await _postService.GetCommentsAsync(id);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpPut("comments/{commentId:long}")]
    [Authorize]
    [ProducesResponseType(typeof(AdviceCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdviceCommentDto>> UpdateComment(long commentId, [FromBody] UpdateAdviceCommentDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _postService.UpdateCommentAsync(commentId, dto, userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
        return Ok(result.Data);
    }

    [HttpDelete("comments/{commentId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteComment(long commentId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _postService.DeleteCommentAsync(commentId, userId);
        if (!result.IsSuccess) return ResultToActionResult(result);
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
