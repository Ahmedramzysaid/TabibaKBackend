namespace DomainLayer.Models;

public class DoctorAdviceVideo
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; }

    public Doctor Doctor { get; set; } = null!;
}
