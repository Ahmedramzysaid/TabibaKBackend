namespace DomainLayer.Models;

public class Call
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string CallerId { get; set; } = string.Empty;
    public string? CalleeId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public DomainLayer.Enums.CallStatus Status { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
