namespace Application.Clients;

public sealed record ListClientsRequest(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListClientsHandler(
    IClientRepository clients)
{
    public async Task<ClientListResult> HandleAsync(
        ListClientsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        var totalCount = await clients.CountAsync(
            search,
            request.IsActive,
            cancellationToken);

        var items = await clients.ListAsync(
            search,
            request.IsActive,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return new ClientListResult(
            items.Select(client => new ClientSummary(
                    client.Id,
                    client.Name,
                    client.ContactPerson,
                    client.Email,
                    client.Phone,
                    client.IsActive))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}

public sealed class ListActiveClientsHandler(
    IClientRepository clients)
{
    public async Task<IReadOnlyList<ClientOption>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await clients.ListActiveAsync(cancellationToken);

        return items
            .Select(client => new ClientOption(
                client.Id,
                client.Name))
            .ToArray();
    }
}
