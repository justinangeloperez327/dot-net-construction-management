using Domain.Commercial;

namespace Application.Commercial;

public static class CommercialCertificationApproval
{
    public const string SubjectType = "CommercialCertification";
}

public sealed record CommercialDeductionDetails(
    Guid Id,
    string Description,
    decimal Amount);

public sealed record CommercialCertificationSummary(
    Guid Id,
    string CertificateNumber,
    string PaymentApplicationNumber,
    string PurchaseOrderNumber,
    string SupplierName,
    DateOnly CertificateDate,
    string Currency,
    decimal ClaimedAmount,
    decimal CertifiedAmount,
    decimal PayableAmount,
    CommercialCertificationStatus Status);

public sealed record CommercialCertificationDetails(
    Guid Id,
    Guid ProjectId,
    Guid PaymentApplicationId,
    string PaymentApplicationNumber,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    string SupplierName,
    string Currency,
    string CertificateNumber,
    DateOnly CertificateDate,
    decimal ClaimedAmount,
    decimal CertifiedAmount,
    decimal UncertifiedAmount,
    decimal RetentionPercent,
    decimal RetentionAmount,
    decimal AdvancePaymentRecoveryAmount,
    decimal OtherDeductionAmount,
    decimal TotalDeductions,
    decimal PayableAmount,
    string? Notes,
    CommercialCertificationStatus Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? ApprovalRequestId,
    IReadOnlyList<CommercialDeductionDetails> OtherDeductions);

public sealed record CommercialCertificationListResult(
    IReadOnlyList<CommercialCertificationSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record CommercialCertificationActionResult(
    bool Succeeded,
    Guid? CertificationId,
    Guid? ApprovalRequestId,
    IReadOnlyList<string> Errors)
{
    public static CommercialCertificationActionResult Success(
        Guid certificationId,
        Guid? approvalRequestId = null) =>
        new(true, certificationId, approvalRequestId, []);

    public static CommercialCertificationActionResult Failure(
        params string[] errors) =>
        new(false, null, null, errors);
}
