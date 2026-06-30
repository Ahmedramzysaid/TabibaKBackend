using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IUserRoleService
{
    Task<Result<string>> AddUserToRole(UserRoleDto userRoleDto);
    Task<Result<string>> RemoveUserFromRole(UserRoleDto userRoleDto);
}
