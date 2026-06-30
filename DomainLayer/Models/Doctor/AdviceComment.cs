using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Models;

public class AdviceComment
{
    public long Id { get; set; }
    public Guid AdvicePostId { get; set; }
    public string UserId { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }

    public DoctorAdvicePost AdvicePost { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
