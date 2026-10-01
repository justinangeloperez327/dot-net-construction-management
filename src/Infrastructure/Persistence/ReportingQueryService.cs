using Application.Reporting;
using Domain.Approvals;
using Domain.Commercial;
using Domain.DailyReports;
using Domain.Deliveries;
using Domain.PaymentApplications;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ReportingQueryService(
    ApplicationDbContext dbContext)
    : IReportingQueryService
{
    public async Task<IReadOnlyList<ReportProjectOption>> ListProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .OrderBy(project => project.ProjectNumber)
            .Select(project => new ReportProjectOption(
                project.Id,
                project.ProjectNumber,
                project.Name,
                project.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReportResult<PortfolioReportRow>> GetPortfolioAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects.AsNoTracking();

        if (filter.ProjectId is Guid projectId)
        {
            query = query.Where(project => project.Id == projectId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ordered = query
            .OrderBy(project => project.ProjectNumber);

        var projects = limit is int maxRows
            ? await ordered.Take(maxRows).ToListAsync(cancellationToken)
            : await ordered.ToListAsync(cancellationToken);

        if (projects.Count == 0)
        {
            return new ReportResult<PortfolioReportRow>(
                [],
                totalCount,
                limit);
        }

        var projectIds = projects
            .Select(project => project.Id)
            .ToArray();

        var clientIds = projects
            .Where(project => project.ClientId.HasValue)
            .Select(project => project.ClientId!.Value)
            .Distinct()
            .ToArray();

        var clients = clientIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.Clients
                .AsNoTracking()
                .Where(client => clientIds.Contains(client.Id))
                .ToDictionaryAsync(
                    client => client.Id,
                    client => client.Name,
                    cancellationToken);

        var latestReports = await dbContext.DailyReports
            .AsNoTracking()
            .Where(report => projectIds.Contains(report.ProjectId))
            .GroupBy(report => report.ProjectId)
            .Select(group => new
            {
                ProjectId = group.Key,
                Date = group.Max(report => report.ReportDate)
            })
            .ToDictionaryAsync(
                item => item.ProjectId,
                item => item.Date,
                cancellationToken);

        var openIssues = await (
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

        var pendingApprovals = await dbContext.ApprovalRequests
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

        var rows = projects
            .Select(project => new PortfolioReportRow(
                project.Id,
                project.ProjectNumber,
                project.Name,
                project.ClientId is Guid clientId
                    ? clients.GetValueOrDefault(clientId)
                    : null,
                project.Location,
                project.Status,
                project.StartDate,
                project.TargetCompletionDate,
                latestReports.TryGetValue(
                    project.Id,
                    out var latestReportDate)
                    ? latestReportDate
                    : null,
                openIssues.GetValueOrDefault(project.Id),
                pendingApprovals.GetValueOrDefault(project.Id)))
            .ToArray();

        return new ReportResult<PortfolioReportRow>(
            rows,
            totalCount,
            limit);
    }

    public async Task<ReportResult<DailySiteReportRow>> GetDailySiteAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.DailyReports.AsNoTracking();

        if (filter.ProjectId is Guid projectId)
        {
            query = query.Where(report => report.ProjectId == projectId);
        }

        if (filter.FromDate is DateOnly fromDate)
        {
            query = query.Where(report => report.ReportDate >= fromDate);
        }

        if (filter.ToDate is DateOnly toDate)
        {
            query = query.Where(report => report.ReportDate <= toDate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ordered = query
            .OrderByDescending(report => report.ReportDate)
            .ThenBy(report => report.ProjectId);

        var reports = limit is int maxRows
            ? await ordered.Take(maxRows).ToListAsync(cancellationToken)
            : await ordered.ToListAsync(cancellationToken);

        if (reports.Count == 0)
        {
            return new ReportResult<DailySiteReportRow>(
                [],
                totalCount,
                limit);
        }

        var reportIds = reports
            .Select(report => report.Id)
            .ToArray();

        var projectIds = reports
            .Select(report => report.ProjectId)
            .Distinct()
            .ToArray();

        var preparerIds = reports
            .Select(report => report.PreparedByUserId)
            .Distinct()
            .ToArray();

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => projectIds.Contains(project.Id))
            .ToDictionaryAsync(
                project => project.Id,
                cancellationToken);

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user => preparerIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => user.Email ?? user.UserName ?? "Unknown user",
                cancellationToken);

        var activityCounts = await dbContext.DailyReportActivities
            .AsNoTracking()
            .Where(item => reportIds.Contains(item.DailyReportId))
            .GroupBy(item => item.DailyReportId)
            .Select(group => new
            {
                ReportId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.ReportId,
                item => item.Count,
                cancellationToken);

        var manpowerCounts = await dbContext.DailyReportManpower
            .AsNoTracking()
            .Where(item => reportIds.Contains(item.DailyReportId))
            .GroupBy(item => item.DailyReportId)
            .Select(group => new
            {
                ReportId = group.Key,
                Headcount = group.Sum(item => item.Headcount)
            })
            .ToDictionaryAsync(
                item => item.ReportId,
                item => item.Headcount,
                cancellationToken);

        var equipmentCounts = await dbContext.DailyReportEquipment
            .AsNoTracking()
            .Where(item => reportIds.Contains(item.DailyReportId))
            .GroupBy(item => item.DailyReportId)
            .Select(group => new
            {
                ReportId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToDictionaryAsync(
                item => item.ReportId,
                item => item.Quantity,
                cancellationToken);

        var openIssueCounts = await dbContext.DailyReportSiteIssues
            .AsNoTracking()
            .Where(item =>
                reportIds.Contains(item.DailyReportId) &&
                item.Status == SiteIssueStatus.Open)
            .GroupBy(item => item.DailyReportId)
            .Select(group => new
            {
                ReportId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.ReportId,
                item => item.Count,
                cancellationToken);

        var rows = reports
            .Select(report =>
            {
                var project = projects[report.ProjectId];

                return new DailySiteReportRow(
                    report.Id,
                    report.ProjectId,
                    project.ProjectNumber,
                    project.Name,
                    report.ReportDate,
                    report.Status,
                    users.GetValueOrDefault(
                        report.PreparedByUserId,
                        "Unknown user"),
                    report.Weather,
                    activityCounts.GetValueOrDefault(report.Id),
                    manpowerCounts.GetValueOrDefault(report.Id),
                    equipmentCounts.GetValueOrDefault(report.Id),
                    openIssueCounts.GetValueOrDefault(report.Id));
            })
            .ToArray();

        return new ReportResult<DailySiteReportRow>(
            rows,
            totalCount,
            limit);
    }

    public async Task<ReportResult<ProcurementReportRow>> GetProcurementAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .AsQueryable();

        if (filter.ProjectId is Guid projectId)
        {
            query = query.Where(order => order.ProjectId == projectId);
        }

        if (filter.FromDate is DateOnly fromDate)
        {
            query = query.Where(order => order.OrderDate >= fromDate);
        }

        if (filter.ToDate is DateOnly toDate)
        {
            query = query.Where(order => order.OrderDate <= toDate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ordered = query
            .OrderByDescending(order => order.OrderDate)
            .ThenBy(order => order.PurchaseOrderNumber);

        var orders = limit is int maxRows
            ? await ordered.Take(maxRows).ToListAsync(cancellationToken)
            : await ordered.ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return new ReportResult<ProcurementReportRow>(
                [],
                totalCount,
                limit);
        }

        var projectIds = orders
            .Select(order => order.ProjectId)
            .Distinct()
            .ToArray();

        var requestIds = orders
            .Select(order => order.PurchaseRequestId)
            .Distinct()
            .ToArray();

        var supplierIds = orders
            .Select(order => order.SupplierId)
            .Distinct()
            .ToArray();

        var orderIds = orders
            .Select(order => order.Id)
            .ToArray();

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => projectIds.Contains(project.Id))
            .ToDictionaryAsync(
                project => project.Id,
                cancellationToken);

        var requests = await dbContext.PurchaseRequests
            .AsNoTracking()
            .Where(request => requestIds.Contains(request.Id))
            .ToDictionaryAsync(
                request => request.Id,
                request => request.RequestNumber,
                cancellationToken);

        var suppliers = await dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplierIds.Contains(supplier.Id))
            .ToDictionaryAsync(
                supplier => supplier.Id,
                supplier => supplier.Name,
                cancellationToken);

        var deliveries = await dbContext.Deliveries
            .AsNoTracking()
            .Where(delivery =>
                orderIds.Contains(delivery.PurchaseOrderId) &&
                delivery.Status == DeliveryStatus.Received)
            .GroupBy(delivery => delivery.PurchaseOrderId)
            .Select(group => new
            {
                PurchaseOrderId = group.Key,
                Count = group.Count(),
                LatestDate = group.Max(delivery => delivery.DeliveryDate)
            })
            .ToDictionaryAsync(
                item => item.PurchaseOrderId,
                cancellationToken);

        var rows = orders
            .Select(order =>
            {
                var project = projects[order.ProjectId];
                deliveries.TryGetValue(
                    order.Id,
                    out var delivery);

                return new ProcurementReportRow(
                    order.Id,
                    order.ProjectId,
                    project.ProjectNumber,
                    project.Name,
                    requests.GetValueOrDefault(
                        order.PurchaseRequestId,
                        "Unknown PR"),
                    order.PurchaseOrderNumber,
                    suppliers.GetValueOrDefault(
                        order.SupplierId,
                        "Unknown supplier"),
                    order.Status,
                    order.OrderDate,
                    order.ExpectedDeliveryDate,
                    order.Currency,
                    order.GrandTotal,
                    delivery?.Count ?? 0,
                    delivery?.LatestDate);
            })
            .ToArray();

        return new ReportResult<ProcurementReportRow>(
            rows,
            totalCount,
            limit);
    }

    public async Task<ReportResult<CommercialReportRow>> GetCommercialAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PaymentApplications
            .AsNoTracking()
            .Include(application => application.Items)
            .AsQueryable();

        if (filter.ProjectId is Guid projectId)
        {
            query = query.Where(application =>
                application.ProjectId == projectId);
        }

        if (filter.FromDate is DateOnly fromDate)
        {
            query = query.Where(application =>
                application.ApplicationDate >= fromDate);
        }

        if (filter.ToDate is DateOnly toDate)
        {
            query = query.Where(application =>
                application.ApplicationDate <= toDate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ordered = query
            .OrderByDescending(application => application.ApplicationDate)
            .ThenBy(application => application.ApplicationNumber);

        var applications = limit is int maxRows
            ? await ordered.Take(maxRows).ToListAsync(cancellationToken)
            : await ordered.ToListAsync(cancellationToken);

        if (applications.Count == 0)
        {
            return new ReportResult<CommercialReportRow>(
                [],
                totalCount,
                limit);
        }

        var projectIds = applications
            .Select(application => application.ProjectId)
            .Distinct()
            .ToArray();

        var orderIds = applications
            .Select(application => application.PurchaseOrderId)
            .Distinct()
            .ToArray();

        var applicationIds = applications
            .Select(application => application.Id)
            .ToArray();

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => projectIds.Contains(project.Id))
            .ToDictionaryAsync(
                project => project.Id,
                cancellationToken);

        var orders = await dbContext.PurchaseOrders
            .AsNoTracking()
            .Where(order => orderIds.Contains(order.Id))
            .ToDictionaryAsync(
                order => order.Id,
                cancellationToken);

        var supplierIds = orders.Values
            .Select(order => order.SupplierId)
            .Distinct()
            .ToArray();

        var suppliers = await dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplierIds.Contains(supplier.Id))
            .ToDictionaryAsync(
                supplier => supplier.Id,
                supplier => supplier.Name,
                cancellationToken);

        var certifications = await dbContext.CommercialCertifications
            .AsNoTracking()
            .Include(certification => certification.OtherDeductions)
            .Where(certification =>
                applicationIds.Contains(certification.PaymentApplicationId))
            .ToDictionaryAsync(
                certification => certification.PaymentApplicationId,
                cancellationToken);

        var rows = applications
            .Select(application =>
            {
                var project = projects[application.ProjectId];
                var order = orders[application.PurchaseOrderId];
                certifications.TryGetValue(
                    application.Id,
                    out var certification);

                return new CommercialReportRow(
                    application.Id,
                    application.ProjectId,
                    project.ProjectNumber,
                    project.Name,
                    order.PurchaseOrderNumber,
                    suppliers.GetValueOrDefault(
                        order.SupplierId,
                        "Unknown supplier"),
                    application.ApplicationNumber,
                    application.ApplicationDate,
                    application.Status,
                    order.Currency,
                    application.ClaimedAmount,
                    certification?.CertificateNumber,
                    certification?.CertificateDate,
                    certification?.Status,
                    certification?.CertifiedAmount,
                    certification?.PayableAmount);
            })
            .ToArray();

        return new ReportResult<CommercialReportRow>(
            rows,
            totalCount,
            limit);
    }
}
