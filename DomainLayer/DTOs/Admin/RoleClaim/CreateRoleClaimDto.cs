namespace DomainLayer.DTOs;

public class CreateRoleClaimDto
{
    public string RoleId { get; set; }

    public string ClaimValue { get; set; } = string.Empty;
}
