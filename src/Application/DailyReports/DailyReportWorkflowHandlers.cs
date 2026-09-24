using Application.Common.Authentication;

namespace Application.DailyReports;

public sealed class SubmitDailyReportHandler(
    IDailyReportRepository reports,
    TimeProvider timeProvider)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await reports.GetByIdAsync(
            reportId,
            cancellationToken);

        if (report is null)
        {
            return DailyReportActionResult.Failure(
                "Daily report was not found.");
        }

        try
        {
            report.Submit(timeProvider.GetUtcNow());
            await reports.SaveChangesAsync(cancellationToken);

            return DailyReportActionResult.Success(report.Id);
        }
        catch (InvalidOperationException exception)
        {
            return DailyReportActionResult.Failure(exception.Message);
        }
    }
}

public sealed record ReviewDailyReportRequest(
    bool Approve,
    string? Comments);

public sealed class ReviewDailyReportHandler(
    IDailyReportRepository reports,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        ReviewDailyReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var report = await reports.GetByIdAsync(
            reportId,
            cancellationToken);

        if (report is null)
        {
            return DailyReportActionResult.Failure(
                "Daily report was not found.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid reviewerUserId)
        {
            return DailyReportActionResult.Failure(
                "An authenticated reviewer is required.");
        }

        try
        {
            if (request.Approve)
            {
                report.Approve(
                    reviewerUserId,
                    timeProvider.GetUtcNow(),
                    request.Comments);
            }
            else
            {
                report.Reject(
                    reviewerUserId,
                    timeProvider.GetUtcNow(),
                    request.Comments ?? string.Empty);
            }

            await reports.SaveChangesAsync(cancellationToken);

            return DailyReportActionResult.Success(report.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException or InvalidOperationException)
        {
            return DailyReportActionResult.Failure(exception.Message);
        }
    }
}
