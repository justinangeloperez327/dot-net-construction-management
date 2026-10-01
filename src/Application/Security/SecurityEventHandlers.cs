namespace Application.Security;

public sealed class ListSecurityEventsHandler(
    ISecurityEventQueryService securityEvents)
{
    public Task<SecurityEventResult> HandleAsync(
        SecurityEventQuery query,
        CancellationToken cancellationToken = default) =>
        securityEvents.ListAsync(query, cancellationToken);
}
