using Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Identity;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole(SystemRoles.Administrator) ||
            context.User.HasClaim(
                PermissionClaimTypes.Permission,
                requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
