using Application.Suppliers;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class SupplierRepository(
    ApplicationDbContext dbContext)
    : ISupplierRepository
{
    public Task<Supplier?> GetByIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Suppliers
            .SingleOrDefaultAsync(
                supplier => supplier.Id == supplierId,
                cancellationToken);
    }

    public Task<bool> SupplierCodeExistsAsync(
        string supplierCode,
        Guid? excludingSupplierId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Suppliers
            .AnyAsync(
                supplier =>
                    supplier.SupplierCode == supplierCode &&
                    (!excludingSupplierId.HasValue ||
                     supplier.Id != excludingSupplierId.Value),
                cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> ListAsync(
        string? search,
        bool? isActive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(
                dbContext.Suppliers.AsNoTracking(),
                search,
                isActive)
            .OrderBy(supplier => supplier.Name)
            .ThenBy(supplier => supplier.SupplierCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplier.IsActive)
            .OrderBy(supplier => supplier.Name)
            .ThenBy(supplier => supplier.SupplierCode)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilter(
                dbContext.Suppliers.AsNoTracking(),
                search,
                isActive)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Suppliers.AddAsync(
            supplier,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Supplier> ApplyFilter(
        IQueryable<Supplier> query,
        string? search,
        bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(supplier =>
                supplier.SupplierCode.Contains(search) ||
                supplier.Name.Contains(search) ||
                (supplier.Category != null &&
                 supplier.Category.Contains(search)) ||
                (supplier.ContactPerson != null &&
                 supplier.ContactPerson.Contains(search)) ||
                (supplier.Email != null &&
                 supplier.Email.Contains(search)) ||
                (supplier.RegistrationNumber != null &&
                 supplier.RegistrationNumber.Contains(search)) ||
                (supplier.TaxRegistrationNumber != null &&
                 supplier.TaxRegistrationNumber.Contains(search)));
        }

        if (isActive is not null)
        {
            query = query.Where(
                supplier => supplier.IsActive == isActive.Value);
        }

        return query;
    }
}
