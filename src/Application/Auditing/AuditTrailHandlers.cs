namespace Application.Auditing;

public sealed class ListAuditTrailHandler(
    IAuditTrailQueryService auditTrail)
{
    public Task<AuditTrailResult> HandleAsync(
        AuditTrailQuery query,
        CancellationToken cancellationToken = default) =>
        auditTrail.ListAsync(
            query,
            cancellationToken);
}

public sealed class ListAuditEntityTypesHandler(
    IAuditTrailQueryService auditTrail)
{
    public Task<IReadOnlyList<string>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        auditTrail.ListEntityTypesAsync(cancellationToken);
}
