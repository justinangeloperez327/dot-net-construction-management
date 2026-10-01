using Domain.PurchaseOrders;
using Xunit;

namespace Domain.Tests;

public sealed class PurchaseOrderTests
{
    [Fact]
    public void Line_and_order_totals_are_derived_from_commercial_fields()
    {
        var order = CreateOrder();

        var item = order.AddItem(
            Guid.NewGuid(),
            "Cement board",
            2m,
            "pcs",
            10m,
            10m,
            5m,
            null);

        Assert.Equal(20m, item.GrossAmount);
        Assert.Equal(2m, item.DiscountAmount);
        Assert.Equal(0.90m, item.TaxAmount);
        Assert.Equal(18.90m, item.TotalAmount);

        Assert.Equal(20m, order.Subtotal);
        Assert.Equal(2m, order.DiscountTotal);
        Assert.Equal(0.90m, order.TaxTotal);
        Assert.Equal(18.90m, order.GrandTotal);
    }

    [Fact]
    public void Submit_requires_positive_order_total()
    {
        var order = CreateOrder();

        order.AddItem(
            Guid.NewGuid(),
            "Free sample",
            1m,
            "pcs",
            0m,
            0m,
            0m,
            null);

        Assert.Throws<InvalidOperationException>(() =>
            order.SubmitForApproval(Guid.NewGuid()));
    }

    [Fact]
    public void Currency_is_normalized_and_validated()
    {
        var order = PurchaseOrder.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PO-001",
            new DateOnly(2026, 10, 1),
            null,
            " aed ",
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Equal("AED", order.Currency);

        Assert.Throws<ArgumentException>(() =>
            PurchaseOrder.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PO-002",
                new DateOnly(2026, 10, 1),
                null,
                "AE",
                null,
                null,
                null,
                null,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Rejected_order_can_be_edited_and_resubmitted()
    {
        var order = CreateOrder();
        var item = order.AddItem(
            Guid.NewGuid(),
            "Cement board",
            2m,
            "pcs",
            10m,
            0m,
            0m,
            null);

        var firstApprovalId = Guid.NewGuid();
        order.SubmitForApproval(firstApprovalId);
        order.ApplyApprovalOutcome(
            firstApprovalId,
            PurchaseOrderStatus.Rejected);

        order.UpdateItem(
            item.Id,
            1m,
            9m,
            0m,
            0m,
            "Revised quantity and price.");

        var secondApprovalId = Guid.NewGuid();
        order.SubmitForApproval(secondApprovalId);

        Assert.Equal(
            PurchaseOrderStatus.PendingApproval,
            order.Status);
        Assert.Equal(secondApprovalId, order.ApprovalRequestId);
    }

    private static PurchaseOrder CreateOrder() =>
        PurchaseOrder.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PO-001",
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 15),
            "AED",
            "Project site",
            null,
            "30 days",
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
