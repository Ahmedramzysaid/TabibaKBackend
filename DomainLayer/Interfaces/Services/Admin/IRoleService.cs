using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IRoleService
{
    Task<Result<RoleDto>> CreateRole(RoleDto roleDto);
    Task<Result<RoleDto>> GetRole(string id);
    Task<Result<PaginatedResult<RoleDto>>> GetAllRoles(int pageNumber = 1, int pageSize = 10);
    Task<Result<RoleDto>> UpdateRole(RoleDto roleDto);
    Task<Result<RoleDto>> DeleteRole(string id);
}
