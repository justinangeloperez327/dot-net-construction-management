using Domain.DailyReports;

namespace Application.DailyReports;

public sealed record SaveManpowerEntryRequest(
    string Trade,
    string? Contractor,
    int Headcount,
    decimal? ManHours,
    string? Remarks);

public sealed record SaveEquipmentEntryRequest(
    string Equipment,
    string? Identifier,
    int Quantity,
    decimal? HoursUsed,
    string? Remarks);

public sealed record SaveSiteIssueRequest(
    string Title,
    string Description,
    string? ActionTaken,
    SiteIssueStatus Status);

public sealed class AddManpowerEntryHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        SaveManpowerEntryRequest request,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.AddManpower(
                request.Trade,
                request.Contractor,
                request.Headcount,
                request.ManHours,
                request.Remarks),
            cancellationToken);
}

public sealed class UpdateManpowerEntryHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid entryId,
        SaveManpowerEntryRequest request,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.UpdateManpower(
                entryId,
                request.Trade,
                request.Contractor,
                request.Headcount,
                request.ManHours,
                request.Remarks),
            cancellationToken);
}

public sealed class RemoveManpowerEntryHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid entryId,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.RemoveManpower(entryId),
            cancellationToken);
}

public sealed class AddEquipmentEntryHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        SaveEquipmentEntryRequest request,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.AddEquipment(
                request.Equipment,
                request.Identifier,
                request.Quantity,
                request.HoursUsed,
                request.Remarks),
            cancellationToken);
}

public sealed class UpdateEquipmentEntryHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid entryId,
        SaveEquipmentEntryRequest request,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.UpdateEquipment(
                entryId,
                request.Equipment,
                request.Identifier,
                request.Quantity,
                request.HoursUsed,
                request.Remarks),
            cancellationToken);
}

public sealed class RemoveEquipmentEntryHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid entryId,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.RemoveEquipment(entryId),
            cancellationToken);
}

public sealed class AddSiteIssueHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        SaveSiteIssueRequest request,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.AddSiteIssue(
                request.Title,
                request.Description,
                request.ActionTaken,
                request.Status),
            cancellationToken);
}

public sealed class UpdateSiteIssueHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid issueId,
        SaveSiteIssueRequest request,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.UpdateSiteIssue(
                issueId,
                request.Title,
                request.Description,
                request.ActionTaken,
                request.Status),
            cancellationToken);
}

public sealed class RemoveSiteIssueHandler(
    IDailyReportRepository reports)
{
    public Task<DailyReportActionResult> HandleAsync(
        Guid reportId,
        Guid issueId,
        CancellationToken cancellationToken = default) =>
        DailyReportResourceHandlerSupport.ChangeAsync(
            reports,
            reportId,
            report => report.RemoveSiteIssue(issueId),
            cancellationToken);
}

internal static class DailyReportResourceHandlerSupport
{
    public static async Task<DailyReportActionResult> ChangeAsync(
        IDailyReportRepository reports,
        Guid reportId,
        Action<DailyReport> change,
        CancellationToken cancellationToken)
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
            change(report);
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
