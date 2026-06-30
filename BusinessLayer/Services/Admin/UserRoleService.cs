using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;

namespace BusinessLayer.Services;

public class UserRoleService : IUserRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<string>> AddUserToRole(UserRoleDto userRoleDto)
    {
        var user = await _userManager.FindByIdAsync(userRoleDto.UserId);
        if (user is null)
            return Result<string>.Failure("User not found", ServiceErrorType.NotFound);

        var role = await _roleManager.FindByIdAsync(userRoleDto.RoleId);
        if (role is null)
            return Result<string>.Failure("Role not found", ServiceErrorType.NotFound);

        var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
        if (isInRole)
            return Result<string>.Failure("User is already in the role", ServiceErrorType.ValidationError);

        var result = await _userManager.AddToRoleAsync(user, role.Name);
        return result.Succeeded
            ? Result<string>.Success("User added to role successfully")
            : Result<string>.Failure("Failed to add user to role", ServiceErrorType.DatabaseError);
    }

    public async Task<Result<string>> RemoveUserFromRole(UserRoleDto userRoleDto)
    {
        var user = await _userManager.FindByIdAsync(userRoleDto.UserId);
        if (user is null)
            return Result<string>.Failure("User not found", ServiceErrorType.NotFound);

        var role = await _roleManager.FindByIdAsync(userRoleDto.RoleId);
        if (role is null)
            return Result<string>.Failure("Role not found", ServiceErrorType.NotFound);

        var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
        if (!isInRole)
            return Result<string>.Failure("User is not in the role", ServiceErrorType.ValidationError);

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
        return result.Succeeded
            ? Result<string>.Success("User removed from role successfully")
            : Result<string>.Failure("Failed to remove user from role", ServiceErrorType.DatabaseError);
    }
}
