namespace Infrastructure.Security;

public sealed class SecurityEventLog
{
    private SecurityEventLog()
    {
    }

    internal SecurityEventLog(
        string eventType,
        Guid? userId,
        string? email,
        string? remoteIpAddress,
        string? userAgent,
        string? requestPath,
        DateTimeOffset occurredAt)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        UserId = userId;
        Email = Normalize(email);
        RemoteIpAddress = Normalize(remoteIpAddress);
        UserAgent = Normalize(userAgent);
        RequestPath = Normalize(requestPath);
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? RemoteIpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? RequestPath { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
