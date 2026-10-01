using Domain.Deliveries;

namespace Application.Deliveries;

public sealed record DeliveryItemDetails(
    Guid Id,
    Guid PurchaseOrderItemId,
    string Description,
    decimal OrderedQuantity,
    decimal PreviouslyReceivedQuantity,
    decimal Quantity,
    decimal RemainingAfterDelivery,
    string Unit,
    string? Remarks);

public sealed record DeliverySummary(
    Guid Id,
    string DeliveryNoteNumber,
    string PurchaseOrderNumber,
    string SupplierName,
    DateOnly DeliveryDate,
    DeliveryStatus Status,
    string CreatedBy,
    string? ReceivedBy,
    DateTimeOffset? ReceivedAt,
    int ItemCount);

public sealed record DeliveryDetails(
    Guid Id,
    Guid ProjectId,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    string SupplierName,
    string DeliveryNoteNumber,
    DateOnly DeliveryDate,
    string? VehicleReference,
    string? Remarks,
    DeliveryStatus Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    string? ReceivedBy,
    DateTimeOffset? ReceivedAt,
    IReadOnlyList<DeliveryItemDetails> Items);

public sealed record DeliveryListResult(
    IReadOnlyList<DeliverySummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record DeliveryActionResult(
    bool Succeeded,
    Guid? DeliveryId,
    IReadOnlyList<string> Errors)
{
    public static DeliveryActionResult Success(Guid deliveryId) =>
        new(true, deliveryId, []);

    public static DeliveryActionResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}
