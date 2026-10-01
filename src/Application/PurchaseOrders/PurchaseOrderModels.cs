using Domain.PurchaseOrders;

namespace Application.PurchaseOrders;

public static class PurchaseOrderApproval
{
    public const string SubjectType = "PurchaseOrder";
}

public sealed record PurchaseOrderItemDetails(
    Guid Id,
    Guid? PurchaseRequestItemId,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    string? Remarks,
    decimal GrossAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount);

public sealed record PurchaseOrderSummary(
    Guid Id,
    string PurchaseOrderNumber,
    string SupplierName,
    string Currency,
    decimal GrandTotal,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    PurchaseOrderStatus Status);

public sealed record PurchaseOrderDetails(
    Guid Id,
    Guid ProjectId,
    Guid PurchaseRequestId,
    string PurchaseRequestNumber,
    Guid SupplierId,
    string SupplierName,
    string PurchaseOrderNumber,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string Currency,
    string? DeliveryAddress,
    string? DeliveryTerms,
    string? PaymentTerms,
    string? Notes,
    PurchaseOrderStatus Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? ApprovalRequestId,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    IReadOnlyList<PurchaseOrderItemDetails> Items);

public sealed record PurchaseOrderListResult(
    IReadOnlyList<PurchaseOrderSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record PurchaseOrderActionResult(
    bool Succeeded,
    Guid? PurchaseOrderId,
    Guid? ApprovalRequestId,
    IReadOnlyList<string> Errors)
{
    public static PurchaseOrderActionResult Success(
        Guid purchaseOrderId,
        Guid? approvalRequestId = null) =>
        new(true, purchaseOrderId, approvalRequestId, []);

    public static PurchaseOrderActionResult Failure(
        params string[] errors) =>
        new(false, null, null, errors);
}
