using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ClinicAPI.Middlewares;

public class SuperAdminAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.IsInRole(Roles.SuperAdmin))
        {
            var pendingRequirements = context.PendingRequirements.ToList();
            foreach (var requirement in pendingRequirements)
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}
