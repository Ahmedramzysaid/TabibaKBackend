namespace DomainLayer.DTOs;

public class DeleteRoleClaimDto
{
    public string RoleId { get; set; } = string.Empty;

    public string ClaimValue { get; set; } = string.Empty;
}
