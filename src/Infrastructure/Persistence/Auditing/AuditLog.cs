namespace Infrastructure.Persistence.Auditing;

public sealed class AuditLog
{
    private AuditLog()
    {
    }

    internal AuditLog(
        Guid? projectId,
        Guid? actorUserId,
        string? actorEmail,
        string action,
        string entityType,
        string entityId,
        string changesJson,
        DateTimeOffset occurredAt)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        ActorUserId = actorUserId;
        ActorEmail = NormalizeOptional(actorEmail);
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        ChangesJson = changesJson;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public Guid? ProjectId { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public string? ActorEmail { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public string ChangesJson { get; private set; } = "{}";

    public DateTimeOffset OccurredAt { get; private set; }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
