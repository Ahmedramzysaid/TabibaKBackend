namespace DomainLayer.DTOs;

public class UpdateRoleClaimDto
{
    public string RoleId { get; set; } = string.Empty;
    public string OldClaimValue { get; set; } = string.Empty;
    public string NewClaimValue { get; set; } = string.Empty;
}
