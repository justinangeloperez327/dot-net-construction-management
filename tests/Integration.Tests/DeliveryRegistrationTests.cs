using Application.Deliveries;
using Domain.Deliveries;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class DeliveryRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeliveryRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Delivery_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IDeliveryRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateDeliveryHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ReceiveDeliveryHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ListDeliveriesHandler>());
    }

    [Fact]
    public void Delivery_note_is_unique_within_purchase_order()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(typeof(Delivery));

        Assert.NotNull(entity);
        Assert.Equal("Deliveries", entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(Delivery.PurchaseOrderId),
                            nameof(Delivery.DeliveryNoteNumber)
                        ]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Delivery_quantity_uses_purchase_precision()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(DeliveryItem));

        var quantity = entity!
            .FindProperty(nameof(DeliveryItem.Quantity));

        Assert.NotNull(quantity);
        Assert.Equal(18, quantity.GetPrecision());
        Assert.Equal(3, quantity.GetScale());
    }
}
