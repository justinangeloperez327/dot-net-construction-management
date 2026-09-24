using Application.Suppliers;
using Domain.Suppliers;
using Xunit;

namespace Application.Tests;

public sealed class SupplierHandlerTests
{
    [Fact]
    public async Task Create_rejects_duplicate_supplier_code()
    {
        var repository = new FakeSupplierRepository
        {
            CodeExists = true
        };

        var handler = new CreateSupplierHandler(repository);

        var result = await handler.HandleAsync(
            new CreateSupplierRequest(
                "SUP-001",
                "Supplier",
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Create_persists_supplier()
    {
        var repository = new FakeSupplierRepository();
        var handler = new CreateSupplierHandler(repository);

        var result = await handler.HandleAsync(
            new CreateSupplierRequest(
                " SUP-001 ",
                "Supplier",
                "MEP",
                null,
                null,
                null,
                null,
                null,
                null));

        Assert.True(result.Succeeded);
        Assert.Single(repository.Added);
        Assert.Equal(
            "SUP-001",
            repository.Added.Single().SupplierCode);
        Assert.Equal(1, repository.SaveCount);
    }

    private sealed class FakeSupplierRepository
        : ISupplierRepository
    {
        public bool CodeExists { get; set; }

        public int SaveCount { get; private set; }

        public List<Supplier> Added { get; } = [];

        public Task<Supplier?> GetByIdAsync(
            Guid supplierId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Supplier?>(null);

        public Task<bool> SupplierCodeExistsAsync(
            string supplierCode,
            Guid? excludingSupplierId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CodeExists);

        public Task<IReadOnlyList<Supplier>> ListAsync(
            string? search,
            bool? isActive,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Supplier>>([]);

        public Task<IReadOnlyList<Supplier>> ListActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Supplier>>([]);

        public Task<int> CountAsync(
            string? search,
            bool? isActive,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Supplier supplier,
            CancellationToken cancellationToken = default)
        {
            Added.Add(supplier);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
