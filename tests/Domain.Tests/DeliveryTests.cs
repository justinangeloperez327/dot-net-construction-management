using Domain.Deliveries;
using Xunit;

namespace Domain.Tests;

public sealed class DeliveryTests
{
    [Fact]
    public void Receive_requires_at_least_one_item()
    {
        var delivery = CreateDelivery();

        Assert.Throws<InvalidOperationException>(() =>
            delivery.Receive(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Received_delivery_is_immutable()
    {
        var delivery = CreateDelivery();

        delivery.AddItem(
            Guid.NewGuid(),
            "Cement board",
            5m,
            "pcs",
            null);

        delivery.Receive(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Equal(DeliveryStatus.Received, delivery.Status);

        Assert.Throws<InvalidOperationException>(() =>
            delivery.UpdateHeader(
                "DN-002",
                new DateOnly(2026, 10, 2),
                null,
                null));

        Assert.Throws<InvalidOperationException>(delivery.Cancel);
    }

    [Fact]
    public void Duplicate_purchase_order_item_is_rejected()
    {
        var delivery = CreateDelivery();
        var purchaseOrderItemId = Guid.NewGuid();

        delivery.AddItem(
            purchaseOrderItemId,
            "Cement board",
            5m,
            "pcs",
            null);

        Assert.Throws<InvalidOperationException>(() =>
            delivery.AddItem(
                purchaseOrderItemId,
                "Cement board",
                2m,
                "pcs",
                null));
    }

    [Fact]
    public void Draft_delivery_can_be_cancelled()
    {
        var delivery = CreateDelivery();

        delivery.Cancel();

        Assert.Equal(DeliveryStatus.Cancelled, delivery.Status);
    }

    private static Delivery CreateDelivery() =>
        Delivery.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DN-001",
            new DateOnly(2026, 10, 1),
            "TRUCK-01",
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
