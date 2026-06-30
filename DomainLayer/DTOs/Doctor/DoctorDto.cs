namespace DomainLayer.DTOs;

public class DoctorDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfRegistration { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string Gender { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string IdNo { get; set; } = string.Empty;
    public string? IdImageFrontUrl { get; set; }
    public string? IdImageBackUrl { get; set; }
    public string? ProfileImageUrl { get; set; }
    public decimal? Price { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? Rating { get; set; }
    public int RatingCount { get; set; }
}
