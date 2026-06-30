namespace DomainLayer.DTOs;

public class DoctorAdvicePostDto
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? DoctorSpecialization { get; set; }
    public string? DoctorProfileImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}
