using Application.Users;
using Domain.DailyReports;

namespace Application.DailyReports;

public sealed record ListDailyReportsRequest(
    Guid ProjectId,
    DailyReportStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListDailyReportsHandler(
    IDailyReportRepository reports,
    IUserDirectory users)
{
    public async Task<DailyReportListResult> HandleAsync(
        ListDailyReportsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await reports.CountForProjectAsync(
            request.ProjectId,
            request.Status,
            cancellationToken);

        var items = await reports.ListForProjectAsync(
            request.ProjectId,
            request.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var userIds = items
            .Select(report => report.PreparedByUserId)
            .Distinct()
            .ToArray();

        var directory = await users.ListByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        return new DailyReportListResult(
            items.Select(report => new DailyReportSummary(
                    report.Id,
                    report.ReportDate,
                    usersById.GetValueOrDefault(
                            report.PreparedByUserId)?.Email
                        ?? "Unknown user",
                    report.Status,
                    report.SubmittedAt))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
