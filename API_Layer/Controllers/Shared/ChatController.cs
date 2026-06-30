using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.DTOs.Chat;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;

    public ChatController(IConversationService conversationService, IMessageService messageService)
    {
        _conversationService = conversationService;
        _messageService = messageService;
    }

    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsSuperAdmin => User.IsInRole(Roles.SuperAdmin);

    [HttpPost("conversations")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConversationDto>> CreateOrGetConversation([FromBody] CreateConversationRequestDto request, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var conv = await _conversationService.CreateOrGetAsync(userId, request ?? new CreateConversationRequestDto(), IsSuperAdmin, cancellationToken);
        if (conv == null) return BadRequest("Cannot create conversation. The patient and doctor must have at least one appointment together. Check DoctorId/PatientId and try again.");
        return Ok(conv);
    }

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetMyConversations(CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var list = await _conversationService.GetMyConversationsAsync(userId, IsSuperAdmin, cancellationToken);
        return Ok(list);
    }

    [HttpGet("conversations/by-participants")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversationsByParticipantIds(
        [FromQuery] string? patientId,
        [FromQuery] string? doctorId,
        CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var list = await _conversationService.GetConversationsByParticipantIdsAsync(patientId, doctorId, userId, IsSuperAdmin, cancellationToken);
        return Ok(list);
    }

    [HttpGet("conversations/{id:guid}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDto>> GetConversationById(Guid id, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var conv = await _conversationService.GetByIdAsync(id, userId, IsSuperAdmin, cancellationToken);
        if (conv == null) return NotFound();
        return Ok(conv);
    }

    [HttpPost("conversations/{id:guid}/messages")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(Guid id, [FromBody] SendMessageRequestDto? body, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (body == null || string.IsNullOrWhiteSpace(body.Content))
            return BadRequest("Content is required.");
        var msg = await _messageService.SendMessageAsync(id, userId, body.Content.Trim(), IsSuperAdmin, cancellationToken);
        if (msg == null) return BadRequest("Conversation not found or you are not a participant.");
        return CreatedAtAction(nameof(GetHistory), new { id }, msg);
    }

    [HttpGet("conversations/{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageHistoryResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageHistoryResponseDto>> GetHistory(Guid id, [FromQuery] long? before, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _messageService.GetHistoryAsync(id, userId, before, limit, IsSuperAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("conversations/{id:guid}/messages/{messageId:long}")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChatMessageDto>> EditMessage(Guid id, long messageId, [FromBody] EditMessageRequestDto? body, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (body == null || string.IsNullOrWhiteSpace(body.Content))
            return BadRequest("Content is required.");
        var msg = await _messageService.EditMessageAsync(id, messageId, userId, body.Content.Trim(), IsSuperAdmin, cancellationToken);
        if (msg == null)
            return BadRequest("Message not found, already deleted, or you are not the sender.");
        return Ok(msg);
    }

    [HttpDelete("conversations/{id:guid}/messages/{messageId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMessage(Guid id, long messageId, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var deleted = await _messageService.DeleteMessageAsync(id, messageId, userId, IsSuperAdmin, cancellationToken);
        if (!deleted)
            return BadRequest("Message not found, already deleted, or you are not the sender.");
        return NoContent();
    }

    [HttpPost("conversations/{id:guid}/messages/{messageId:long}/delivered")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAsDelivered(Guid id, long messageId, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var updated = await _messageService.MarkAsDeliveredAsync(id, messageId, userId, IsSuperAdmin, cancellationToken);
        if (!updated) return BadRequest("Message not found, you are not the recipient, or already delivered/read.");
        return NoContent();
    }

    [HttpPost("conversations/{id:guid}/messages/{messageId:long}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAsRead(Guid id, long messageId, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var updated = await _messageService.MarkAsReadAsync(id, messageId, userId, IsSuperAdmin, cancellationToken);
        if (!updated) return BadRequest("Message not found or you are not the recipient.");
        return NoContent();
    }
}
