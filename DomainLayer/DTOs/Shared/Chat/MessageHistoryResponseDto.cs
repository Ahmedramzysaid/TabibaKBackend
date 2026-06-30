namespace DomainLayer.DTOs.Chat;

public class MessageHistoryResponseDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
    public long? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
