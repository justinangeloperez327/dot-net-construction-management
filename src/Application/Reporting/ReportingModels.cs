using Domain.Commercial;
using Domain.DailyReports;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;

namespace Application.Reporting;

public enum ReportKind
{
    Portfolio = 1,
    DailySite = 2,
    Procurement = 3,
    Commercial = 4
}

public sealed record ReportingFilter(
    Guid? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null);

public sealed record ReportProjectOption(
    Guid Id,
    string ProjectNumber,
    string Name,
    ProjectStatus Status);

public sealed record ReportResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int? Limit)
{
    public bool IsTruncated =>
        Limit is int limit && TotalCount > limit;
}

public sealed record PortfolioReportRow(
    Guid ProjectId,
    string ProjectNumber,
    string ProjectName,
    string? ClientName,
    string? Location,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    DateOnly? LatestDailyReportDate,
    int OpenSiteIssues,
    int PendingApprovals);

public sealed record DailySiteReportRow(
    Guid ReportId,
    Guid ProjectId,
    string ProjectNumber,
    string ProjectName,
    DateOnly ReportDate,
    DailyReportStatus Status,
    string PreparedBy,
    string? Weather,
    int ActivityCount,
    int ManpowerHeadcount,
    int EquipmentQuantity,
    int OpenSiteIssues);

public sealed record ProcurementReportRow(
    Guid PurchaseOrderId,
    Guid ProjectId,
    string ProjectNumber,
    string ProjectName,
    string PurchaseRequestNumber,
    string PurchaseOrderNumber,
    string SupplierName,
    PurchaseOrderStatus Status,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string Currency,
    decimal PurchaseOrderTotal,
    int DeliveryCount,
    DateOnly? LatestDeliveryDate);

public sealed record CommercialReportRow(
    Guid PaymentApplicationId,
    Guid ProjectId,
    string ProjectNumber,
    string ProjectName,
    string PurchaseOrderNumber,
    string SupplierName,
    string ApplicationNumber,
    DateOnly ApplicationDate,
    PaymentApplicationStatus ApplicationStatus,
    string Currency,
    decimal ClaimedAmount,
    string? CertificateNumber,
    DateOnly? CertificateDate,
    CommercialCertificationStatus? CertificationStatus,
    decimal? CertifiedAmount,
    decimal? PayableAmount);

public sealed record ReportCsvDocument(
    string FileName,
    byte[] Content);
