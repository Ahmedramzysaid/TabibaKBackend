using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<RoleDto>> CreateRole(RoleDto roleDto)
    {
        if (await _roleManager.RoleExistsAsync(roleDto.Name))
            return Result<RoleDto>.Failure("Role already exists", ServiceErrorType.Conflict);

        var role = new IdentityRole(roleDto.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return Result<RoleDto>.Failure("Failed to create role", ServiceErrorType.DatabaseError);

        roleDto.Id = role.Id;
        return Result<RoleDto>.Success(roleDto);
    }

    public async Task<Result<RoleDto>> GetRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return Result<RoleDto>.Failure("Role not found", ServiceErrorType.NotFound);

        var roleDto = new RoleDto { Id = role.Id, Name = role.Name };
        return Result<RoleDto>.Success(roleDto);
    }

    public async Task<Result<PaginatedResult<RoleDto>>> GetAllRoles(int pageNumber = 1, int pageSize = 10)
    {
        var roles = await _roleManager.Roles.ToListAsync();
        if (roles.Count == 0)
            return Result<PaginatedResult<RoleDto>>.Failure("Roles not found", ServiceErrorType.NotFound);

        var rolesDto = roles.Select(r =>
            new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

        var pagedItems = rolesDto.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var paged = PaginatedResult<RoleDto>.Create(pagedItems, rolesDto.Count, pageNumber, pageSize);
        return Result<PaginatedResult<RoleDto>>.Success(paged);
    }

    public async Task<Result<RoleDto>> UpdateRole(RoleDto roleDto)
    {
        if (string.IsNullOrEmpty(roleDto.Name) || string.IsNullOrWhiteSpace(roleDto.Name))
            return Result<RoleDto>.Failure("Role name is required", ServiceErrorType.ValidationError);

        var role = await _roleManager.FindByIdAsync(roleDto.Id);
        if (role is null)
            return Result<RoleDto>.Failure("Role not found", ServiceErrorType.NotFound);

        role.Name = roleDto.Name;
        var result = await _roleManager.UpdateAsync(role);
        return result.Succeeded
            ? Result<RoleDto>.Success()
            : Result<RoleDto>.Failure("Failed to update role", ServiceErrorType.DatabaseError);
    }

    public async Task<Result<RoleDto>> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return Result<RoleDto>.Failure("Role not found", ServiceErrorType.NotFound);

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded
            ? Result<RoleDto>.Success()
            : Result<RoleDto>.Failure("Failed to delete role", ServiceErrorType.DatabaseError);
    }
}
