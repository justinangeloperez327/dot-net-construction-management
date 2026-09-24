using Domain.PurchaseRequests;

namespace Application.PurchaseRequests;

public static class PurchaseRequestApproval
{
    public const string SubjectType = "PurchaseRequest";
}

public sealed record PurchaseRequestItemDetails(
    Guid Id,
    string Description,
    decimal Quantity,
    string Unit,
    string? Remarks);

public sealed record PurchaseRequestSummary(
    Guid Id,
    string RequestNumber,
    string Title,
    DateOnly? RequiredByDate,
    PurchaseRequestStatus Status,
    string RequestedBy,
    DateTimeOffset CreatedAt,
    int ItemCount);

public sealed record PurchaseRequestDetails(
    Guid Id,
    Guid ProjectId,
    string RequestNumber,
    string Title,
    DateOnly? RequiredByDate,
    string? Purpose,
    PurchaseRequestStatus Status,
    string RequestedBy,
    DateTimeOffset CreatedAt,
    Guid? ApprovalRequestId,
    IReadOnlyList<PurchaseRequestItemDetails> Items);

public sealed record PurchaseRequestListResult(
    IReadOnlyList<PurchaseRequestSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record PurchaseRequestActionResult(
    bool Succeeded,
    Guid? PurchaseRequestId,
    Guid? ApprovalRequestId,
    IReadOnlyList<string> Errors)
{
    public static PurchaseRequestActionResult Success(
        Guid purchaseRequestId,
        Guid? approvalRequestId = null) =>
        new(true, purchaseRequestId, approvalRequestId, []);

    public static PurchaseRequestActionResult Failure(
        params string[] errors) =>
        new(false, null, null, errors);
}
