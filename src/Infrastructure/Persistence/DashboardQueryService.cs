using Application.Dashboard;
using Domain.Approvals;
using Domain.Commercial;
using Domain.DailyReports;
using Domain.Deliveries;
using Domain.Documents;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DashboardQueryService(
    ApplicationDbContext dbContext)
    : IDashboardQueryService
{
    public async Task<PortfolioDashboardSnapshot> GetPortfolioAsync(
        Guid? userId,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var activeProjects = await dbContext.Projects
            .AsNoTracking()
            .CountAsync(
                project => project.Status == ProjectStatus.Active,
                cancellationToken);

        var closedProjects = await dbContext.Projects
            .AsNoTracking()
            .CountAsync(
                project => project.Status == ProjectStatus.Closed,
                cancellationToken);

        var overdueProjects = await dbContext.Projects
            .AsNoTracking()
            .CountAsync(
                project =>
                    project.Status == ProjectStatus.Active &&
                    project.TargetCompletionDate != null &&
                    project.TargetCompletionDate < today,
                cancellationToken);

        var openSiteIssues = await dbContext.DailyReportSiteIssues
            .AsNoTracking()
            .CountAsync(
                issue => issue.Status == SiteIssueStatus.Open,
                cancellationToken);

        var pendingApprovalsAssignedToUser = userId is Guid currentUserId
            ? await dbContext.ApprovalSteps
                .AsNoTracking()
                .CountAsync(
                    step =>
                        step.ApproverUserId == currentUserId &&
                        step.Status == ApprovalStepStatus.Pending,
                    cancellationToken)
            : 0;

        var pendingPurchaseRequests = await dbContext.PurchaseRequests
            .AsNoTracking()
            .CountAsync(
                request =>
                    request.Status == PurchaseRequestStatus.PendingApproval,
                cancellationToken);

        var pendingPurchaseOrders = await dbContext.PurchaseOrders
            .AsNoTracking()
            .CountAsync(
                order =>
                    order.Status == PurchaseOrderStatus.PendingApproval,
                cancellationToken);

        var draftDeliveries = await dbContext.Deliveries
            .AsNoTracking()
            .CountAsync(
                delivery => delivery.Status == DeliveryStatus.Draft,
                cancellationToken);

        var pendingPaymentApplications = await dbContext.PaymentApplications
            .AsNoTracking()
            .CountAsync(
                application =>
                    application.Status
                        == PaymentApplicationStatus.PendingApproval,
                cancellationToken);

        var pendingCommercialCertifications =
            await dbContext.CommercialCertifications
                .AsNoTracking()
                .CountAsync(
                    certification =>
                        certification.Status
                            == CommercialCertificationStatus.PendingApproval,
                    cancellationToken);

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Status == ProjectStatus.Active)
            .OrderBy(project =>
                project.TargetCompletionDate == null ? 1 : 0)
            .ThenBy(project => project.TargetCompletionDate)
            .ThenBy(project => project.ProjectNumber)
            .Take(8)
            .Select(project => new
            {
                project.Id,
                project.ProjectNumber,
                project.Name,
                project.Location,
                project.TargetCompletionDate
            })
            .ToListAsync(cancellationToken);

        var projectIds = projects
            .Select(project => project.Id)
            .ToArray();

        var issueCounts = projectIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await (
                    from issue in dbContext.DailyReportSiteIssues.AsNoTracking()
                    join report in dbContext.DailyReports.AsNoTracking()
                        on issue.DailyReportId equals report.Id
                    where projectIds.Contains(report.ProjectId) &&
                          issue.Status == SiteIssueStatus.Open
                    group issue by report.ProjectId
                    into projectIssues
                    select new
                    {
                        ProjectId = projectIssues.Key,
                        Count = projectIssues.Count()
                    })
                .ToDictionaryAsync(
                    item => item.ProjectId,
                    item => item.Count,
                    cancellationToken);

        var approvalCounts = projectIds.Length == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.ApprovalRequests
                .AsNoTracking()
                .Where(request =>
                    request.ProjectId != null &&
                    projectIds.Contains(request.ProjectId.Value) &&
                    request.Status == ApprovalRequestStatus.Pending)
                .GroupBy(request => request.ProjectId!.Value)
                .Select(group => new
                {
                    ProjectId = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(
                    item => item.ProjectId,
                    item => item.Count,
                    cancellationToken);

        var latestReportDates = projectIds.Length == 0
            ? new Dictionary<Guid, DateOnly>()
            : await dbContext.DailyReports
                .AsNoTracking()
                .Where(report => projectIds.Contains(report.ProjectId))
                .GroupBy(report => report.ProjectId)
                .Select(group => new
                {
                    ProjectId = group.Key,
                    ReportDate = group.Max(report => report.ReportDate)
                })
                .ToDictionaryAsync(
                    item => item.ProjectId,
                    item => item.ReportDate,
                    cancellationToken);

        var cards = projects
            .Select(project => new DashboardProjectCard(
                project.Id,
                project.ProjectNumber,
                project.Name,
                project.Location,
                project.TargetCompletionDate,
                project.TargetCompletionDate is DateOnly target &&
                    target < today,
                issueCounts.GetValueOrDefault(project.Id),
                approvalCounts.GetValueOrDefault(project.Id),
                latestReportDates.TryGetValue(
                    project.Id,
                    out var latestReportDate)
                    ? latestReportDate
                    : null))
            .ToArray();

        return new PortfolioDashboardSnapshot(
            activeProjects,
            closedProjects,
            overdueProjects,
            openSiteIssues,
            pendingApprovalsAssignedToUser,
            pendingPurchaseRequests,
            pendingPurchaseOrders,
            draftDeliveries,
            pendingPaymentApplications,
            pendingCommercialCertifications,
            cards);
    }

    public async Task<ProjectDashboardSnapshot?> GetProjectAsync(
        Guid projectId,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Id == projectId)
            .Select(project => new
            {
                project.Id,
                project.ProjectNumber,
                project.Name,
                project.Location,
                project.Status,
                project.StartDate,
                project.TargetCompletionDate
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return null;
        }

        var dailyReportCounts = await dbContext.DailyReports
            .AsNoTracking()
            .Where(report => report.ProjectId == projectId)
            .GroupBy(report => report.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var latestDailyReport = await dbContext.DailyReports
            .AsNoTracking()
            .Where(report => report.ProjectId == projectId)
            .OrderByDescending(report => report.ReportDate)
            .ThenByDescending(report => report.SubmittedAt)
            .Select(report => new
            {
                report.ReportDate,
                report.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        var openSiteIssues = await (
                from issue in dbContext.DailyReportSiteIssues.AsNoTracking()
                join report in dbContext.DailyReports.AsNoTracking()
                    on issue.DailyReportId equals report.Id
                where report.ProjectId == projectId &&
                      issue.Status == SiteIssueStatus.Open
                select issue)
            .CountAsync(cancellationToken);

        var documentCounts = await dbContext.Documents
            .AsNoTracking()
            .Where(document => document.ProjectId == projectId)
            .GroupBy(document => document.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var purchaseRequestCounts = await dbContext.PurchaseRequests
            .AsNoTracking()
            .Where(request => request.ProjectId == projectId)
            .GroupBy(request => request.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var purchaseOrderCounts = await dbContext.PurchaseOrders
            .AsNoTracking()
            .Where(order => order.ProjectId == projectId)
            .GroupBy(order => order.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var deliveryCounts = await dbContext.Deliveries
            .AsNoTracking()
            .Where(delivery => delivery.ProjectId == projectId)
            .GroupBy(delivery => delivery.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var paymentApplicationCounts = await dbContext.PaymentApplications
            .AsNoTracking()
            .Where(application => application.ProjectId == projectId)
            .GroupBy(application => application.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var certificationCounts = await dbContext.CommercialCertifications
            .AsNoTracking()
            .Where(certification => certification.ProjectId == projectId)
            .GroupBy(certification => certification.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken);

        var pendingApprovalRequests = await dbContext.ApprovalRequests
            .AsNoTracking()
            .CountAsync(
                request =>
                    request.ProjectId == projectId &&
                    request.Status == ApprovalRequestStatus.Pending,
                cancellationToken);

        var targetDate = project.TargetCompletionDate;
        var daysToTarget = targetDate is null
            ? null
            : targetDate.Value.DayNumber - today.DayNumber;

        return new ProjectDashboardSnapshot(
            project.Id,
            project.ProjectNumber,
            project.Name,
            project.Location,
            project.Status,
            project.StartDate,
            targetDate,
            project.Status == ProjectStatus.Active &&
                targetDate is DateOnly target &&
                target < today,
            daysToTarget,
            latestDailyReport?.ReportDate,
            latestDailyReport?.Status,
            openSiteIssues,
            pendingApprovalRequests,
            new DailyReportWorkflowCounts(
                Sum(dailyReportCounts),
                Count(dailyReportCounts, DailyReportStatus.Draft),
                Count(dailyReportCounts, DailyReportStatus.Submitted),
                Count(dailyReportCounts, DailyReportStatus.Approved),
                Count(dailyReportCounts, DailyReportStatus.Rejected)),
            new DocumentWorkflowCounts(
                Sum(documentCounts),
                Count(documentCounts, DocumentStatus.Active),
                Count(documentCounts, DocumentStatus.Archived)),
            ToApprovalCounts(purchaseRequestCounts),
            ToApprovalCounts(purchaseOrderCounts),
            new DeliveryWorkflowCounts(
                Sum(deliveryCounts),
                Count(deliveryCounts, DeliveryStatus.Draft),
                Count(deliveryCounts, DeliveryStatus.Received),
                Count(deliveryCounts, DeliveryStatus.Cancelled)),
            ToApprovalCounts(paymentApplicationCounts),
            ToApprovalCounts(certificationCounts));
    }

    private static ApprovalWorkflowCounts ToApprovalCounts(
        IReadOnlyDictionary<PurchaseRequestStatus, int> counts) =>
        new(
            Sum(counts),
            Count(counts, PurchaseRequestStatus.Draft),
            Count(counts, PurchaseRequestStatus.PendingApproval),
            Count(counts, PurchaseRequestStatus.Approved),
            Count(counts, PurchaseRequestStatus.Rejected),
            Count(counts, PurchaseRequestStatus.Cancelled));

    private static ApprovalWorkflowCounts ToApprovalCounts(
        IReadOnlyDictionary<PurchaseOrderStatus, int> counts) =>
        new(
            Sum(counts),
            Count(counts, PurchaseOrderStatus.Draft),
            Count(counts, PurchaseOrderStatus.PendingApproval),
            Count(counts, PurchaseOrderStatus.Approved),
            Count(counts, PurchaseOrderStatus.Rejected),
            Count(counts, PurchaseOrderStatus.Cancelled));

    private static ApprovalWorkflowCounts ToApprovalCounts(
        IReadOnlyDictionary<PaymentApplicationStatus, int> counts) =>
        new(
            Sum(counts),
            Count(counts, PaymentApplicationStatus.Draft),
            Count(counts, PaymentApplicationStatus.PendingApproval),
            Count(counts, PaymentApplicationStatus.Approved),
            Count(counts, PaymentApplicationStatus.Rejected),
            Count(counts, PaymentApplicationStatus.Cancelled));

    private static ApprovalWorkflowCounts ToApprovalCounts(
        IReadOnlyDictionary<CommercialCertificationStatus, int> counts) =>
        new(
            Sum(counts),
            Count(counts, CommercialCertificationStatus.Draft),
            Count(counts, CommercialCertificationStatus.PendingApproval),
            Count(counts, CommercialCertificationStatus.Approved),
            Count(counts, CommercialCertificationStatus.Rejected),
            Count(counts, CommercialCertificationStatus.Cancelled));

    private static int Count<TStatus>(
        IReadOnlyDictionary<TStatus, int> counts,
        TStatus status)
        where TStatus : notnull =>
        counts.GetValueOrDefault(status);

    private static int Sum<TStatus>(
        IReadOnlyDictionary<TStatus, int> counts)
        where TStatus : notnull =>
        counts.Values.Sum();
}
