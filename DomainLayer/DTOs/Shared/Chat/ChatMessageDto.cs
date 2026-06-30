using DomainLayer.Enums;

namespace DomainLayer.DTOs.Chat;

public class ChatMessageDto
{
    public long Id { get; set; }
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? EditedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public MessageDeliveryStatus? YourStatus { get; set; } // For the current user as recipient
}
