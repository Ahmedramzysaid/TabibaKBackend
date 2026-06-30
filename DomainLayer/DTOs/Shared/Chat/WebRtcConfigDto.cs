namespace DomainLayer.DTOs.Chat;

public class WebRtcConfigDto
{
    public string StunUrl { get; set; } = "stun:stun.l.google.com:19302";
    public string? TurnUrl { get; set; }
    public string? TurnUsername { get; set; }
    public string? TurnCredential { get; set; }
}
