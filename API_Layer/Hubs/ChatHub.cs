using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClinicAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;

    public ChatHub(IConversationService conversationService, IMessageService messageService)
    {
        _conversationService = conversationService;
        _messageService = messageService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        var isSuperAdmin = Context.User?.IsInRole(Roles.SuperAdmin) ?? false;
        var allowedIds = await _conversationService.GetAllowedConversationIdsForUserAsync(userId, isSuperAdmin, Context.ConnectionAborted);
        foreach (var id in allowedIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{id}");
        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        var isSuperAdmin = Context.User?.IsInRole(Roles.SuperAdmin) ?? false;
        var allowed = await _conversationService.GetAllowedConversationIdsForUserAsync(userId, isSuperAdmin, Context.ConnectionAborted);
        if (allowed.Contains(conversationId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
    }

    public async Task MarkAsDelivered(Guid conversationId, long messageId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        var isSuperAdmin = Context.User?.IsInRole(Roles.SuperAdmin) ?? false;
        var updated = await _messageService.MarkAsDeliveredAsync(conversationId, messageId, userId, isSuperAdmin, Context.ConnectionAborted);
        if (!updated) return;
        await Clients.Group($"conversation:{conversationId}").SendAsync("MessageDelivered", conversationId, messageId, userId, Context.ConnectionAborted);
    }

    public async Task MarkAsRead(Guid conversationId, long messageId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        var isSuperAdmin = Context.User?.IsInRole(Roles.SuperAdmin) ?? false;
        var updated = await _messageService.MarkAsReadAsync(conversationId, messageId, userId, isSuperAdmin, Context.ConnectionAborted);
        if (!updated) return;
        await Clients.Group($"conversation:{conversationId}").SendAsync("MessageRead", conversationId, messageId, userId, Context.ConnectionAborted);
    }
}
