using DomainLayer.Enums;

namespace DomainLayer.Models;

public class MessageStatus
{
    public long MessageId { get; set; }
    public string RecipientId { get; set; } = string.Empty;
    public MessageDeliveryStatus Status { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Message Message { get; set; } = null!;
    public ApplicationUser Recipient { get; set; } = null!;
}
