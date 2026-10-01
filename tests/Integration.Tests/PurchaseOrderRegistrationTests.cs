using Application.PurchaseOrders;
using Domain.PurchaseOrders;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class PurchaseOrderRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchaseOrderRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Purchase_order_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IPurchaseOrderRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreatePurchaseOrderHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<SubmitPurchaseOrderHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetPurchaseOrderHandler>());
    }

    [Fact]
    public void Purchase_order_number_is_company_wide_unique()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(PurchaseOrder));

        Assert.NotNull(entity);
        Assert.Equal("PurchaseOrders", entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [nameof(PurchaseOrder.PurchaseOrderNumber)]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Commercial_numbers_use_fixed_decimal_precision()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(PurchaseOrderItem));

        Assert.NotNull(entity);

        var quantity = entity.FindProperty(
            nameof(PurchaseOrderItem.Quantity));
        var unitPrice = entity.FindProperty(
            nameof(PurchaseOrderItem.UnitPrice));
        var discount = entity.FindProperty(
            nameof(PurchaseOrderItem.DiscountPercent));
        var tax = entity.FindProperty(
            nameof(PurchaseOrderItem.TaxPercent));

        Assert.Equal(18, quantity!.GetPrecision());
        Assert.Equal(3, quantity.GetScale());

        Assert.Equal(18, unitPrice!.GetPrecision());
        Assert.Equal(4, unitPrice.GetScale());

        Assert.Equal(5, discount!.GetPrecision());
        Assert.Equal(2, discount.GetScale());

        Assert.Equal(5, tax!.GetPrecision());
        Assert.Equal(2, tax.GetScale());
    }
}
