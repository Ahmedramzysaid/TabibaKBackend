namespace DomainLayer.DTOs;

public class PatientCreateResponseDto
{
    public PatientDto Patient { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresOn { get; set; }
    public bool IsAuthenticated { get; set; } = true;
}
