using System.Security.Claims;
using Application.Common.Authorization;
using Application.Roles;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public sealed class RoleAdministration(
    RoleManager<IdentityRole<Guid>> roleManager,
    ApplicationDbContext dbContext)
    : IRoleAdministration
{
    public async Task<IReadOnlyList<RoleSummary>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new RoleSummary(
                role.Id,
                role.Name ?? string.Empty))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleDetails?> GetAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByIdAsync(roleId.ToString());

        if (role is null)
        {
            return null;
        }

        var claims = await roleManager.GetClaimsAsync(role);

        var permissions = claims
            .Where(claim => claim.Type == PermissionClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Where(Permissions.All.Contains)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission)
            .ToArray();

        return new RoleDetails(
            role.Id,
            role.Name ?? string.Empty,
            permissions,
            IsAdministrator(role));
    }

    public async Task<RoleActionResult> CreateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        name = name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return RoleActionResult.Failure("Role name is required.");
        }

        if (string.Equals(
                name,
                SystemRoles.Administrator,
                StringComparison.OrdinalIgnoreCase))
        {
            return RoleActionResult.Failure(
                "The Administrator role is reserved.");
        }

        var role = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        var result = await roleManager.CreateAsync(role);

        return result.Succeeded
            ? RoleActionResult.Success(role.Id)
            : RoleActionResult.Failure(
                result.Errors.Select(error => error.Description));
    }

    public async Task<RoleActionResult> RenameAsync(
        Guid roleId,
        string name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByIdAsync(roleId.ToString());

        if (role is null)
        {
            return RoleActionResult.Failure("Role was not found.");
        }

        if (IsAdministrator(role))
        {
            return RoleActionResult.Failure(
                "The Administrator role cannot be renamed.");
        }

        name = name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return RoleActionResult.Failure("Role name is required.");
        }

        if (string.Equals(
                name,
                SystemRoles.Administrator,
                StringComparison.OrdinalIgnoreCase))
        {
            return RoleActionResult.Failure(
                "The Administrator role name is reserved.");
        }

        role.Name = name;

        var result = await roleManager.UpdateAsync(role);

        return result.Succeeded
            ? RoleActionResult.Success(role.Id)
            : RoleActionResult.Failure(
                result.Errors.Select(error => error.Description));
    }

    public async Task<RoleActionResult> DeleteAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());

        if (role is null)
        {
            return RoleActionResult.Failure("Role was not found.");
        }

        if (IsAdministrator(role))
        {
            return RoleActionResult.Failure(
                "The Administrator role cannot be deleted.");
        }

        var isAssigned = await dbContext.UserRoles
            .AnyAsync(
                userRole => userRole.RoleId == roleId,
                cancellationToken);

        if (isAssigned)
        {
            return RoleActionResult.Failure(
                "Remove this role from all users before deleting it.");
        }

        var result = await roleManager.DeleteAsync(role);

        return result.Succeeded
            ? RoleActionResult.Success(role.Id)
            : RoleActionResult.Failure(
                result.Errors.Select(error => error.Description));
    }

    public async Task<RoleActionResult> SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByIdAsync(roleId.ToString());

        if (role is null)
        {
            return RoleActionResult.Failure("Role was not found.");
        }

        if (IsAdministrator(role))
        {
            return RoleActionResult.Failure(
                "Administrator permissions are managed by the system.");
        }

        var requested = permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var invalid = requested
            .Except(Permissions.All, StringComparer.Ordinal)
            .ToArray();

        if (invalid.Length > 0)
        {
            return RoleActionResult.Failure(
                $"Unknown permission: {string.Join(", ", invalid)}");
        }

        var currentClaims = await roleManager.GetClaimsAsync(role);

        var currentPermissions = currentClaims
            .Where(claim => claim.Type == PermissionClaimTypes.Permission)
            .ToArray();

        foreach (var claim in currentPermissions
                     .Where(claim => !requested.Contains(
                         claim.Value,
                         StringComparer.Ordinal)))
        {
            var removeResult = await roleManager.RemoveClaimAsync(
                role,
                claim);

            if (!removeResult.Succeeded)
            {
                return RoleActionResult.Failure(
                    removeResult.Errors.Select(error => error.Description));
            }
        }

        var existing = currentPermissions
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var permission in requested.Where(
                     permission => !existing.Contains(permission)))
        {
            var addResult = await roleManager.AddClaimAsync(
                role,
                new Claim(
                    PermissionClaimTypes.Permission,
                    permission));

            if (!addResult.Succeeded)
            {
                return RoleActionResult.Failure(
                    addResult.Errors.Select(error => error.Description));
            }
        }

        return RoleActionResult.Success(role.Id);
    }

    private static bool IsAdministrator(
        IdentityRole<Guid> role)
    {
        return string.Equals(
            role.Name,
            SystemRoles.Administrator,
            StringComparison.OrdinalIgnoreCase);
    }
}
