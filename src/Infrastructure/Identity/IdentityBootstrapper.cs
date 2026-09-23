using System.Security.Claims;
using Application.Common.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public static class IdentityBootstrapper
{
    public static async Task EnsureBootstrapAdministratorAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin requires both Email and Password.");
        }

        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var role = await roleManager.FindByNameAsync(
            SystemRoles.Administrator);

        if (role is null)
        {
            role = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = SystemRoles.Administrator
            };

            var roleResult = await roleManager.CreateAsync(role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        roleResult.Errors.Select(
                            error => error.Description)));
            }
        }

        await EnsureAdministratorPermissionsAsync(
            roleManager,
            role);

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                IsActive = true
            };

            var userResult = await userManager.CreateAsync(
                user,
                password);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        userResult.Errors.Select(
                            error => error.Description)));
            }
        }

        if (!user.IsActive)
        {
            user.IsActive = true;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(
                user,
                SystemRoles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user,
                SystemRoles.Administrator);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        roleResult.Errors.Select(
                            error => error.Description)));
            }
        }
    }

    private static async Task EnsureAdministratorPermissionsAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        IdentityRole<Guid> role)
    {
        var claims = await roleManager.GetClaimsAsync(role);

        var existing = claims
            .Where(claim => claim.Type == PermissionClaimTypes.Permission)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var permission in Permissions.All.Where(
                     permission => !existing.Contains(permission)))
        {
            var result = await roleManager.AddClaimAsync(
                role,
                new Claim(
                    PermissionClaimTypes.Permission,
                    permission));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        result.Errors.Select(
                            error => error.Description)));
            }
        }
    }
}
