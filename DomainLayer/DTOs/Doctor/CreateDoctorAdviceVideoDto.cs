namespace DomainLayer.DTOs;

public class CreateDoctorAdviceVideoDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
}
