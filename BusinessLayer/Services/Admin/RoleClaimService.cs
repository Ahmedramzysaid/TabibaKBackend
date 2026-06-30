using System.Security.Claims;
using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class RoleClaimService : IRoleClaimService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleClaimService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<string>> CreateRoleClaim(CreateRoleClaimDto roleClaimDto)
    {
        if (string.IsNullOrWhiteSpace(roleClaimDto.ClaimValue))
            return Result<string>.Failure("Claim value is required", ServiceErrorType.ValidationError);

        var role = await _roleManager.FindByIdAsync(roleClaimDto.RoleId);
        if (role is null)
            return Result<string>.Failure("Role not found", ServiceErrorType.NotFound);

        var existingClaims = await _roleManager.GetClaimsAsync(role);

        var exists = existingClaims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == roleClaimDto.ClaimValue
        );

        if (exists)
            return Result<string>.Failure("Claim already exists for this role", ServiceErrorType.ValidationError);

        var claim = new Claim(ClaimConstants.Permission, roleClaimDto.ClaimValue);

        var result = await _roleManager.AddClaimAsync(role, claim);

        if (!result.Succeeded)
            return Result<string>.Failure("Failed to create role claim", ServiceErrorType.DatabaseError);

        return Result<string>.Success("Role claim created successfully");
    }

    public async Task<Result<string>> UpdateRoleClaim(UpdateRoleClaimDto roleClaimDto)
    {
        if (string.IsNullOrWhiteSpace(roleClaimDto.OldClaimValue) ||
            string.IsNullOrWhiteSpace(roleClaimDto.NewClaimValue))
            return Result<string>.Failure("Old and new claim values are required", ServiceErrorType.ValidationError);

        var role = await _roleManager.FindByIdAsync(roleClaimDto.RoleId);
        if (role is null)
            return Result<string>.Failure("Role not found", ServiceErrorType.NotFound);

        var roleClaims = await _roleManager.GetClaimsAsync(role);

        var matchedClaim = roleClaims.FirstOrDefault(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == roleClaimDto.OldClaimValue);

        if (matchedClaim is null)
            return Result<string>.Failure("Old claim not found for this role", ServiceErrorType.NotFound);

        var isNewClaimExists = roleClaims.Any(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == roleClaimDto.NewClaimValue);

        if (isNewClaimExists)
            return Result<string>.Failure("New claim already exists for this role", ServiceErrorType.Conflict);

        var removeResult = await _roleManager.RemoveClaimAsync(role, matchedClaim);
        if (!removeResult.Succeeded)
            return Result<string>.Failure("Failed to remove old claim", ServiceErrorType.DatabaseError);

        var newClaim = new Claim(ClaimConstants.Permission, roleClaimDto.NewClaimValue);
        var addResult = await _roleManager.AddClaimAsync(role, newClaim);
        if (!addResult.Succeeded)
            return Result<string>.Failure("Failed to add new claim", ServiceErrorType.DatabaseError);

        return Result<string>.Success("Role claim updated successfully");
    }

    public async Task<Result<string>> DeleteRoleClaimAsync(DeleteRoleClaimDto roleClaimDto)
    {
        if (string.IsNullOrWhiteSpace(roleClaimDto.ClaimValue))
            return Result<string>.Failure("Claim value is required", ServiceErrorType.ValidationError);

        var role = await _roleManager.FindByIdAsync(roleClaimDto.RoleId);
        if (role is null)
            return Result<string>.Failure("Role not found", ServiceErrorType.NotFound);

        var roleClaims = await _roleManager.GetClaimsAsync(role);

        var matchedClaim = roleClaims.FirstOrDefault(c =>
            c.Type == ClaimConstants.Permission &&
            c.Value == roleClaimDto.ClaimValue);

        if (matchedClaim is null)
            return Result<string>.Failure("Claim not found for this role", ServiceErrorType.NotFound);

        var result = await _roleManager.RemoveClaimAsync(role, matchedClaim);
        if (!result.Succeeded)
            return Result<string>.Failure("Failed to remove claim", ServiceErrorType.DatabaseError);

        return Result<string>.Success("Role claim removed successfully");
    }

    public async Task<Result<PaginatedResult<RoleClaimDto>>> GetAllRoleClaims(int pageNumber = 1, int pageSize = 10)
    {
        var roles = await _roleManager.Roles.ToListAsync();
        if (roles.Count == 0)
            return Result<PaginatedResult<RoleClaimDto>>.Failure("No roles found", ServiceErrorType.NotFound);

        var roleClaims = new List<RoleClaimDto>();

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);

            foreach (var claim in claims)
                roleClaims.Add(new RoleClaimDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });
        }

        if (roleClaims.Count == 0)
            return Result<PaginatedResult<RoleClaimDto>>.Failure("No role claims found", ServiceErrorType.NotFound);

        var pagedItems = roleClaims.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var paged = PaginatedResult<RoleClaimDto>.Create(pagedItems, roleClaims.Count, pageNumber, pageSize);
        return Result<PaginatedResult<RoleClaimDto>>.Success(paged);
    }

    public async Task<Result<List<RoleClaimDto>>> GetClaimsByRoleId(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return Result<List<RoleClaimDto>>.Failure("Role ID is required", ServiceErrorType.ValidationError);

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null)
            return Result<List<RoleClaimDto>>.Failure("Role not found", ServiceErrorType.NotFound);

        var claims = await _roleManager.GetClaimsAsync(role);

        if (claims.Count == 0)
            return Result<List<RoleClaimDto>>.Failure("No claims found for this role", ServiceErrorType.NotFound);

        var roleClaims = claims.Select(c => new RoleClaimDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            ClaimType = c.Type,
            ClaimValue = c.Value
        }).ToList();

        return Result<List<RoleClaimDto>>.Success(roleClaims);
    }
}
