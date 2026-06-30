using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Current (old) password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
    [MaxLength(256)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{6,256}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character (@$!%*?&#)")]
    public string NewPassword { get; set; } = string.Empty;
}
