namespace Application.Common.Authentication;

public interface ICurrentUser
{
    ValueTask<CurrentUserInfo> GetAsync(
        CancellationToken cancellationToken = default);
}

public sealed record CurrentUserInfo(
    Guid? UserId,
    string? Email,
    bool IsAuthenticated);
