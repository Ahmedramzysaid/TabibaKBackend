using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class UserRoleDto
{
    [Required]
    [RegularExpression(@"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$",
        ErrorMessage = "Invalid GUID format for UserId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$",
        ErrorMessage = "Invalid GUID format for RoleId")]
    public string RoleId { get; set; } = string.Empty;
}
