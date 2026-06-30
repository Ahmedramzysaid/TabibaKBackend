namespace DomainLayer.DTOs;

public class DoctorAdviceVideoDto
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; }
}
