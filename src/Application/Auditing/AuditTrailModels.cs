namespace Application.Auditing;

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3
}

public sealed record AuditTrailQuery(
    Guid? ProjectId = null,
    AuditAction? Action = null,
    string? EntityType = null,
    string? Actor = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50);

public sealed record AuditTrailItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    Guid? ProjectId,
    Guid? ActorUserId,
    string Actor,
    AuditAction Action,
    string EntityType,
    string EntityId,
    string ChangesJson);

public sealed record AuditTrailResult(
    IReadOnlyList<AuditTrailItem> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(
            1,
            (int)Math.Ceiling(TotalCount / (double)PageSize));
}
