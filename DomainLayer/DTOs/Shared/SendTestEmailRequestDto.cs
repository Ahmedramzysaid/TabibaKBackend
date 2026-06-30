using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class SendTestEmailRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    public string? Message { get; set; }
}
