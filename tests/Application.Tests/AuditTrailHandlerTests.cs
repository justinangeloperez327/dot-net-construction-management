using Application.Auditing;
using Xunit;

namespace Application.Tests;

public sealed class AuditTrailHandlerTests
{
    [Fact]
    public async Task List_handler_forwards_filters()
    {
        var service = new FakeAuditTrailQueryService();
        var handler = new ListAuditTrailHandler(service);
        var projectId = Guid.NewGuid();

        var query = new AuditTrailQuery(
            projectId,
            AuditAction.Updated,
            "PurchaseOrder",
            "commercial@example.com",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            Page: 2,
            PageSize: 25);

        await handler.HandleAsync(
            query,
            TestContext.Current.CancellationToken);

        Assert.Equal(query, service.LastQuery);
    }

    [Fact]
    public async Task Entity_type_handler_returns_query_service_values()
    {
        var service = new FakeAuditTrailQueryService
        {
            EntityTypes =
            [
                "Project",
                "PurchaseOrder"
            ]
        };

        var handler = new ListAuditEntityTypesHandler(service);

        var result = await handler.HandleAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            ["Project", "PurchaseOrder"],
            result);
    }

    private sealed class FakeAuditTrailQueryService
        : IAuditTrailQueryService
    {
        public AuditTrailQuery? LastQuery { get; private set; }

        public IReadOnlyList<string> EntityTypes { get; set; } = [];

        public Task<AuditTrailResult> ListAsync(
            AuditTrailQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;

            return Task.FromResult(
                new AuditTrailResult(
                    [],
                    query.Page,
                    query.PageSize,
                    0));
        }

        public Task<IReadOnlyList<string>> ListEntityTypesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(EntityTypes);
    }
}
