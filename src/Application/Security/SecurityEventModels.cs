namespace Application.Security;

public enum SecurityEventType
{
    LoginSucceeded = 1,
    LoginFailed = 2,
    LoginLockedOut = 3,
    Logout = 4,
    RateLimitRejected = 5
}

public sealed record SecurityEventRecord(
    SecurityEventType EventType,
    Guid? UserId,
    string? Email,
    string? RemoteIpAddress,
    string? UserAgent,
    string? RequestPath,
    DateTimeOffset OccurredAt);

public sealed record SecurityEventQuery(
    SecurityEventType? EventType = null,
    string? Email = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50);

public sealed record SecurityEventItem(
    Guid Id,
    SecurityEventType EventType,
    Guid? UserId,
    string? Email,
    string? RemoteIpAddress,
    string? UserAgent,
    string? RequestPath,
    DateTimeOffset OccurredAt);

public sealed record SecurityEventResult(
    IReadOnlyList<SecurityEventItem> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
