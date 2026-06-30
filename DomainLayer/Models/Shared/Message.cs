using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Models;

public class Message
{
    public long Id { get; set; }
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    [MaxLength(8000)]
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? EditedAtUtc { get; set; }
    public bool IsDeleted { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
    public ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
}
