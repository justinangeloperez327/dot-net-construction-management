using Domain.DailyReports;
using Domain.Projects;

namespace Application.Dashboard;

public sealed record DashboardProjectCard(
    Guid Id,
    string ProjectNumber,
    string Name,
    string? Location,
    DateOnly? TargetCompletionDate,
    bool IsOverdue,
    int OpenSiteIssues,
    int PendingApprovals,
    DateOnly? LatestDailyReportDate);

public sealed record PortfolioDashboardSnapshot(
    int ActiveProjects,
    int ClosedProjects,
    int OverdueProjects,
    int OpenSiteIssues,
    int PendingApprovalsAssignedToUser,
    int PendingPurchaseRequests,
    int PendingPurchaseOrders,
    int DraftDeliveries,
    int PendingPaymentApplications,
    int PendingCommercialCertifications,
    IReadOnlyList<DashboardProjectCard> ActiveProjectCards);

public sealed record ApprovalWorkflowCounts(
    int Total,
    int Draft,
    int PendingApproval,
    int Approved,
    int Rejected,
    int Cancelled);

public sealed record DeliveryWorkflowCounts(
    int Total,
    int Draft,
    int Received,
    int Cancelled);

public sealed record DailyReportWorkflowCounts(
    int Total,
    int Draft,
    int Submitted,
    int Approved,
    int Rejected);

public sealed record DocumentWorkflowCounts(
    int Total,
    int Active,
    int Archived);

public sealed record ProjectDashboardSnapshot(
    Guid ProjectId,
    string ProjectNumber,
    string Name,
    string? Location,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    bool IsOverdue,
    int? DaysToTarget,
    DateOnly? LatestDailyReportDate,
    DailyReportStatus? LatestDailyReportStatus,
    int OpenSiteIssues,
    int PendingApprovalRequests,
    DailyReportWorkflowCounts DailyReports,
    DocumentWorkflowCounts Documents,
    ApprovalWorkflowCounts PurchaseRequests,
    ApprovalWorkflowCounts PurchaseOrders,
    DeliveryWorkflowCounts Deliveries,
    ApprovalWorkflowCounts PaymentApplications,
    ApprovalWorkflowCounts CommercialCertifications);
