namespace DomainLayer.Models;

public class AdviceLike
{
    public int Id { get; set; }
    public Guid AdvicePostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public DoctorAdvicePost AdvicePost { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
