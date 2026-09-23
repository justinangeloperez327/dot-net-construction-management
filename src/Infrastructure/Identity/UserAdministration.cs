using Application.Common.Authorization;
using Application.Users;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public sealed class UserAdministration(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ApplicationDbContext dbContext)
    : IUserAdministration
{
    public async Task<IReadOnlyList<UserSummary>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Select(user => new UserSummary(
                user.Id,
                user.Email ?? string.Empty,
                user.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetails?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new UserDetails(
            user.Id,
            user.Email ?? string.Empty,
            user.IsActive,
            roles.OrderBy(role => role).ToArray());
    }

    public async Task<UserActionResult> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            return UserActionResult.Failure("Email is required.");
        }

        var requestedRoles = request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roleError = await ValidateRolesAsync(requestedRoles);

        if (roleError is not null)
        {
            return UserActionResult.Failure(roleError);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            IsActive = true
        };

        var result = await userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            return UserActionResult.Failure(
                result.Errors.Select(error => error.Description));
        }

        if (requestedRoles.Length > 0)
        {
            var roleResult = await userManager.AddToRolesAsync(
                user,
                requestedRoles);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);

                return UserActionResult.Failure(
                    roleResult.Errors.Select(error => error.Description));
            }
        }

        return UserActionResult.Success(user.Id);
    }

    public async Task<UserActionResult> UpdateAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return UserActionResult.Failure("User was not found.");
        }

        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            return UserActionResult.Failure("Email is required.");
        }

        user.Email = email;
        user.UserName = email;

        var result = await userManager.UpdateAsync(user);

        return result.Succeeded
            ? UserActionResult.Success(user.Id)
            : UserActionResult.Failure(
                result.Errors.Select(error => error.Description));
    }

    public async Task<UserActionResult> SetActiveAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return UserActionResult.Failure("User was not found.");
        }

        if (!isActive &&
            await userManager.IsInRoleAsync(
                user,
                SystemRoles.Administrator) &&
            !await HasAnotherActiveAdministratorAsync(
                user.Id,
                cancellationToken))
        {
            return UserActionResult.Failure(
                "At least one active Administrator account is required.");
        }

        user.IsActive = isActive;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return UserActionResult.Failure(
                updateResult.Errors.Select(error => error.Description));
        }

        await userManager.UpdateSecurityStampAsync(user);

        return UserActionResult.Success(user.Id);
    }

    public async Task<UserActionResult> SetRolesAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return UserActionResult.Failure("User was not found.");
        }

        var requestedRoles = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roleError = await ValidateRolesAsync(requestedRoles);

        if (roleError is not null)
        {
            return UserActionResult.Failure(roleError);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var removingAdministrator =
            currentRoles.Contains(
                SystemRoles.Administrator,
                StringComparer.OrdinalIgnoreCase) &&
            !requestedRoles.Contains(
                SystemRoles.Administrator,
                StringComparer.OrdinalIgnoreCase);

        if (removingAdministrator &&
            !await HasAnotherActiveAdministratorAsync(
                user.Id,
                cancellationToken))
        {
            return UserActionResult.Failure(
                "At least one active Administrator account is required.");
        }

        var toRemove = currentRoles
            .Except(requestedRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var toAdd = requestedRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (toRemove.Length > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(
                user,
                toRemove);

            if (!removeResult.Succeeded)
            {
                return UserActionResult.Failure(
                    removeResult.Errors.Select(error => error.Description));
            }
        }

        if (toAdd.Length > 0)
        {
            var addResult = await userManager.AddToRolesAsync(
                user,
                toAdd);

            if (!addResult.Succeeded)
            {
                return UserActionResult.Failure(
                    addResult.Errors.Select(error => error.Description));
            }
        }

        await userManager.UpdateSecurityStampAsync(user);

        return UserActionResult.Success(user.Id);
    }

    private async Task<string?> ValidateRolesAsync(
        IReadOnlyCollection<string> roles)
    {
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                return $"Role '{role}' does not exist.";
            }
        }

        return null;
    }

    private async Task<bool> HasAnotherActiveAdministratorAsync(
        Guid excludedUserId,
        CancellationToken cancellationToken)
    {
        var administratorRole = await roleManager.FindByNameAsync(
            SystemRoles.Administrator);

        if (administratorRole is null)
        {
            return false;
        }

        return await dbContext.UserRoles
            .Where(userRole =>
                userRole.RoleId == administratorRole.Id &&
                userRole.UserId != excludedUserId)
            .Join(
                dbContext.Users.Where(user => user.IsActive),
                userRole => userRole.UserId,
                user => user.Id,
                (_, user) => user)
            .AnyAsync(cancellationToken);
    }
}
