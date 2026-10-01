namespace Application.Auditing;

public interface IAuditTrailQueryService
{
    Task<AuditTrailResult> ListAsync(
        AuditTrailQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListEntityTypesAsync(
        CancellationToken cancellationToken = default);
}
