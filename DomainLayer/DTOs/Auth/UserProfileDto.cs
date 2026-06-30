namespace DomainLayer.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime DateOfRegistration { get; set; }
    public List<string> Roles { get; set; } = new();
    
    public string? Specialization { get; set; }
    public string? IdNo { get; set; }
    
    public bool IsPatient { get; set; }
    public bool IsDoctor { get; set; }
}
