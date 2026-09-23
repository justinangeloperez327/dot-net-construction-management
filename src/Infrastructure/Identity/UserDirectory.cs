using Application.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public sealed class UserDirectory(
    ApplicationDbContext dbContext)
    : IUserDirectory
{
    public Task<UserDirectoryEntry?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserDirectoryEntry(
                user.Id,
                user.Email ?? string.Empty,
                user.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDirectoryEntry>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .OrderBy(user => user.Email)
            .Select(user => new UserDirectoryEntry(
                user.Id,
                user.Email ?? string.Empty,
                user.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDirectoryEntry>> ListByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new UserDirectoryEntry(
                user.Id,
                user.Email ?? string.Empty,
                user.IsActive))
            .ToListAsync(cancellationToken);
    }
}
