using Application.Suppliers;
using Domain.Suppliers;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class SupplierRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SupplierRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Supplier_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<ISupplierRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateSupplierHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<UpdateSupplierHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ListActiveSuppliersHandler>());
    }

    [Fact]
    public void Supplier_code_is_unique()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(Supplier));

        Assert.NotNull(entity);
        Assert.Equal(
            "Suppliers",
            entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [nameof(Supplier.SupplierCode)]));

        Assert.True(index.IsUnique);
    }
}
