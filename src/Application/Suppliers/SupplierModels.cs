namespace Application.Suppliers;

public sealed record SupplierSummary(
    Guid Id,
    string SupplierCode,
    string Name,
    string? Category,
    string? ContactPerson,
    string? Email,
    string? Phone,
    bool IsActive);

public sealed record SupplierDetails(
    Guid Id,
    string SupplierCode,
    string Name,
    string? Category,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? RegistrationNumber,
    string? TaxRegistrationNumber,
    bool IsActive);

public sealed record SupplierOption(
    Guid Id,
    string SupplierCode,
    string Name);

public sealed record SupplierListResult(
    IReadOnlyList<SupplierSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record SupplierActionResult(
    bool Succeeded,
    Guid? SupplierId,
    IReadOnlyList<string> Errors)
{
    public static SupplierActionResult Success(Guid supplierId) =>
        new(true, supplierId, []);

    public static SupplierActionResult Failure(params string[] errors) =>
        new(false, null, errors);
}
