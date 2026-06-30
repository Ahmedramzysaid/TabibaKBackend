namespace DomainLayer.Models;

public class ChatAuditLog
{
    public long Id { get; set; }
    public Guid ConversationId { get; set; }
    public string ActorId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g. MessageSent, MessageRead, ConversationCreated
    public long? MessageId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
