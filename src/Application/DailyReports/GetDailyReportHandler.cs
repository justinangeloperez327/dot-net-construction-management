using Application.Users;

namespace Application.DailyReports;

public sealed class GetDailyReportHandler(
    IDailyReportRepository reports,
    IUserDirectory users)
{
    public async Task<DailyReportDetails?> HandleAsync(
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await reports.GetByIdAsync(
            reportId,
            cancellationToken);

        if (report is null)
        {
            return null;
        }

        var userIds = new HashSet<Guid>
        {
            report.PreparedByUserId
        };

        if (report.ReviewedByUserId is Guid reviewerId)
        {
            userIds.Add(reviewerId);
        }

        var directory = await users.ListByIdsAsync(
            userIds.ToArray(),
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        var preparedBy = usersById.GetValueOrDefault(
                report.PreparedByUserId)?.Email
            ?? "Unknown user";

        string? reviewedBy = null;

        if (report.ReviewedByUserId is Guid reviewedById)
        {
            reviewedBy = usersById.GetValueOrDefault(
                    reviewedById)?.Email
                ?? "Unknown user";
        }

        return new DailyReportDetails(
            report.Id,
            report.ProjectId,
            report.ReportDate,
            preparedBy,
            report.Weather,
            report.Remarks,
            report.Status,
            report.SubmittedAt,
            reviewedBy,
            report.ReviewedAt,
            report.ReviewComments,
            report.Activities
                .OrderBy(activity => activity.WorkArea)
                .ThenBy(activity => activity.Activity)
                .Select(activity => new DailyReportActivityDetails(
                    activity.Id,
                    activity.WorkArea,
                    activity.Activity,
                    activity.Status,
                    activity.ProgressPercent,
                    activity.Remarks))
                .ToArray());
    }
}
