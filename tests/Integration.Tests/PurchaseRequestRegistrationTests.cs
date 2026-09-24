using Application.Common.Persistence;
using Application.PurchaseRequests;
using Domain.PurchaseRequests;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class PurchaseRequestRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchaseRequestRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Purchase_request_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IPurchaseRequestRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IUnitOfWork>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreatePurchaseRequestHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<SubmitPurchaseRequestHandler>());
    }

    [Fact]
    public void Request_number_is_unique_within_project()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(PurchaseRequest));

        Assert.NotNull(entity);
        Assert.Equal("PurchaseRequests", entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(PurchaseRequest.ProjectId),
                            nameof(PurchaseRequest.RequestNumber)
                        ]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Item_quantity_uses_fixed_decimal_precision()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(PurchaseRequestItem));

        var quantity = entity!
            .FindProperty(nameof(PurchaseRequestItem.Quantity));

        Assert.NotNull(quantity);
        Assert.Equal(18, quantity.GetPrecision());
        Assert.Equal(3, quantity.GetScale());
    }
}
