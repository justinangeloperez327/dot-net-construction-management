namespace Application.Clients;

public sealed record ClientSummary(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    bool IsActive);

public sealed record ClientDetails(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);

public sealed record ClientOption(
    Guid Id,
    string Name);

public sealed record ClientListResult(
    IReadOnlyList<ClientSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record ClientActionResult(
    bool Succeeded,
    Guid? ClientId,
    IReadOnlyList<string> Errors)
{
    public static ClientActionResult Success(Guid clientId) =>
        new(true, clientId, []);

    public static ClientActionResult Failure(params string[] errors) =>
        new(false, null, errors);
}
