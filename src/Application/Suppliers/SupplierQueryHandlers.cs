namespace Application.Suppliers;

public sealed class GetSupplierHandler(
    ISupplierRepository suppliers)
{
    public async Task<SupplierDetails?> HandleAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var supplier = await suppliers.GetByIdAsync(
            supplierId,
            cancellationToken);

        return supplier is null
            ? null
            : new SupplierDetails(
                supplier.Id,
                supplier.SupplierCode,
                supplier.Name,
                supplier.Category,
                supplier.ContactPerson,
                supplier.Email,
                supplier.Phone,
                supplier.Address,
                supplier.RegistrationNumber,
                supplier.TaxRegistrationNumber,
                supplier.IsActive);
    }
}

public sealed record ListSuppliersRequest(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListSuppliersHandler(
    ISupplierRepository suppliers)
{
    public async Task<SupplierListResult> HandleAsync(
        ListSuppliersRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        var totalCount = await suppliers.CountAsync(
            search,
            request.IsActive,
            cancellationToken);

        var items = await suppliers.ListAsync(
            search,
            request.IsActive,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return new SupplierListResult(
            items.Select(supplier => new SupplierSummary(
                    supplier.Id,
                    supplier.SupplierCode,
                    supplier.Name,
                    supplier.Category,
                    supplier.ContactPerson,
                    supplier.Email,
                    supplier.Phone,
                    supplier.IsActive))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}

public sealed class ListActiveSuppliersHandler(
    ISupplierRepository suppliers)
{
    public async Task<IReadOnlyList<SupplierOption>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var suppliersList = await suppliers.ListActiveAsync(
            cancellationToken);

        return suppliersList
            .Select(supplier => new SupplierOption(
                supplier.Id,
                supplier.SupplierCode,
                supplier.Name))
            .ToArray();
    }
}
