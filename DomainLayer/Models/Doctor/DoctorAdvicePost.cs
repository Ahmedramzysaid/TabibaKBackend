namespace DomainLayer.Models;

public class DoctorAdvicePost
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsPublished { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public ICollection<AdviceLike> Likes { get; set; } = new List<AdviceLike>();
    public ICollection<AdviceComment> Comments { get; set; } = new List<AdviceComment>();
}
