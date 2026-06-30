namespace DomainLayer.DTOs;

public class CreateDoctorAdvicePostDto
{
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}
