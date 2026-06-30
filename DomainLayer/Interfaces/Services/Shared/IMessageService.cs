using DomainLayer.DTOs.Chat;

namespace DomainLayer.Interfaces.Services;

public interface IMessageService
{
    Task<ChatMessageDto?> SendMessageAsync(Guid conversationId, string senderId, string content, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<MessageHistoryResponseDto> GetHistoryAsync(Guid conversationId, string userId, long? before, int limit, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<ChatMessageDto?> EditMessageAsync(Guid conversationId, long messageId, string userId, string newContent, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<bool> DeleteMessageAsync(Guid conversationId, long messageId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<bool> MarkAsDeliveredAsync(Guid conversationId, long messageId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(Guid conversationId, long messageId, string userId, bool isSuperAdmin = false, CancellationToken cancellationToken = default);
}
