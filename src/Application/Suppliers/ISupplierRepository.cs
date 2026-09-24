using Domain.Suppliers;

namespace Application.Suppliers;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    Task<bool> SupplierCodeExistsAsync(
        string supplierCode,
        Guid? excludingSupplierId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Supplier>> ListAsync(
        string? search,
        bool? isActive,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Supplier>> ListActiveAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
