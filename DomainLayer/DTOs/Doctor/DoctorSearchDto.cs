namespace DomainLayer.DTOs;

public class DoctorSearchDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Email { get; set; }
    public decimal? Rating { get; set; }
    public int TotalAppointments { get; set; }
    public bool IsAvailable { get; set; }
}
