using Domain.DailyReports;

namespace Application.DailyReports;

public sealed record UpdateDailyReportRequest(
    string? Weather,
    string? Remarks);

public sealed class UpdateDailyReportHandler(
    IDailyReportRepository reports)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        UpdateDailyReportRequest request,
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
            report.UpdateHeader(
                request.Weather,
                request.Remarks);

            await reports.SaveChangesAsync(cancellationToken);

            return DailyReportActionResult.Success(report.Id);
        }
        catch (InvalidOperationException exception)
        {
            return DailyReportActionResult.Failure(exception.Message);
        }
    }
}

public sealed record SaveDailyReportActivityRequest(
    string WorkArea,
    string Activity,
    DailyActivityStatus Status,
    decimal? ProgressPercent,
    string? Remarks);

public sealed class AddDailyReportActivityHandler(
    IDailyReportRepository reports)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        SaveDailyReportActivityRequest request,
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
            report.AddActivity(
                request.WorkArea,
                request.Activity,
                request.Status,
                request.ProgressPercent,
                request.Remarks);

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

public sealed class UpdateDailyReportActivityHandler(
    IDailyReportRepository reports)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid activityId,
        SaveDailyReportActivityRequest request,
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
            report.UpdateActivity(
                activityId,
                request.WorkArea,
                request.Activity,
                request.Status,
                request.ProgressPercent,
                request.Remarks);

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

public sealed class RemoveDailyReportActivityHandler(
    IDailyReportRepository reports)
{
    public async Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid activityId,
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
            report.RemoveActivity(activityId);
            await reports.SaveChangesAsync(cancellationToken);

            return DailyReportActionResult.Success(report.Id);
        }
        catch (InvalidOperationException exception)
        {
            return DailyReportActionResult.Failure(exception.Message);
        }
    }
}
