namespace Application.Users;

public interface IUserDirectory
{
    Task<UserDirectoryEntry?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDirectoryEntry>> ListActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDirectoryEntry>> ListByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
}

public sealed record UserDirectoryEntry(
    Guid Id,
    string Email,
    bool IsActive);
