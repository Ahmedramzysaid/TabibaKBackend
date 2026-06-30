using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class RoleDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}
