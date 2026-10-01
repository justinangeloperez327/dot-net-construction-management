using Domain.PaymentApplications;

namespace Application.PaymentApplications;

public static class PaymentApplicationApproval
{
    public const string SubjectType = "PaymentApplication";
}

public sealed record AvailablePaymentApplicationItem(
    Guid PurchaseOrderItemId,
    string Description,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal CommittedClaimQuantity,
    decimal AvailableClaimQuantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent);

public sealed record PaymentApplicationItemDetails(
    Guid Id,
    Guid PurchaseOrderItemId,
    string Description,
    decimal ReceivedQuantity,
    decimal PreviouslyCommittedClaimQuantity,
    decimal ClaimedQuantity,
    decimal RemainingClaimableQuantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    string? Remarks,
    decimal GrossAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount);

public sealed record PaymentApplicationSummary(
    Guid Id,
    string ApplicationNumber,
    string PurchaseOrderNumber,
    string SupplierName,
    DateOnly ApplicationDate,
    string Currency,
    decimal ClaimedAmount,
    PaymentApplicationStatus Status,
    string CreatedBy);

public sealed record PaymentApplicationDetails(
    Guid Id,
    Guid ProjectId,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid SupplierId,
    string SupplierName,
    string Currency,
    string ApplicationNumber,
    DateOnly ApplicationDate,
    DateOnly? PeriodFrom,
    DateOnly? PeriodTo,
    string? Notes,
    PaymentApplicationStatus Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? ApprovalRequestId,
    decimal GrossAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ClaimedAmount,
    IReadOnlyList<PaymentApplicationItemDetails> Items);

public sealed record PaymentApplicationListResult(
    IReadOnlyList<PaymentApplicationSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record PaymentApplicationActionResult(
    bool Succeeded,
    Guid? PaymentApplicationId,
    Guid? ApprovalRequestId,
    IReadOnlyList<string> Errors)
{
    public static PaymentApplicationActionResult Success(
        Guid paymentApplicationId,
        Guid? approvalRequestId = null) =>
        new(true, paymentApplicationId, approvalRequestId, []);

    public static PaymentApplicationActionResult Failure(
        params string[] errors) =>
        new(false, null, null, errors);
}
