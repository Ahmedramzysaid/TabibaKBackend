namespace DomainLayer.DTOs;

public class NearbyDoctorDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public decimal? Price { get; set; }
    public decimal? Rating { get; set; }
    public int RatingCount { get; set; }
    public double DistanceKm { get; set; }
    public double Score { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
