namespace DomainLayer.DTOs.Chat;

public class CreateConversationRequestDto
{
    public string? DoctorId { get; set; }
    public string? PatientId { get; set; }
}
