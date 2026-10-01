namespace Application.Security;

public interface ISecurityEventRecorder
{
    Task RecordAsync(
        SecurityEventRecord securityEvent,
        CancellationToken cancellationToken = default);
}

public interface ISecurityEventQueryService
{
    Task<SecurityEventResult> ListAsync(
        SecurityEventQuery query,
        CancellationToken cancellationToken = default);
}
