using Domain.DailyReports;

namespace Application.DailyReports;

public sealed record DailyReportActivityDetails(
    Guid Id,
    string WorkArea,
    string Activity,
    DailyActivityStatus Status,
    decimal? ProgressPercent,
    string? Remarks);

public sealed record ManpowerEntryDetails(
    Guid Id,
    string Trade,
    string? Contractor,
    int Headcount,
    decimal? ManHours,
    string? Remarks);

public sealed record EquipmentEntryDetails(
    Guid Id,
    string Equipment,
    string? Identifier,
    int Quantity,
    decimal? HoursUsed,
    string? Remarks);

public sealed record SiteIssueDetails(
    Guid Id,
    string Title,
    string Description,
    string? ActionTaken,
    SiteIssueStatus Status);

public sealed record DailyReportSummary(
    Guid Id,
    DateOnly ReportDate,
    string PreparedBy,
    DailyReportStatus Status,
    DateTimeOffset? SubmittedAt);

public sealed record DailyReportDetails(
    Guid Id,
    Guid ProjectId,
    DateOnly ReportDate,
    string PreparedBy,
    string? Weather,
    string? Remarks,
    DailyReportStatus Status,
    DateTimeOffset? SubmittedAt,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAt,
    string? ReviewComments,
    IReadOnlyList<DailyReportActivityDetails> Activities,
    IReadOnlyList<ManpowerEntryDetails> ManpowerEntries,
    IReadOnlyList<EquipmentEntryDetails> EquipmentEntries,
    IReadOnlyList<SiteIssueDetails> SiteIssues);

public sealed record DailyReportListResult(
    IReadOnlyList<DailyReportSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record DailyReportActionResult(
    bool Succeeded,
    Guid? ReportId,
    IReadOnlyList<string> Errors)
{
    public static DailyReportActionResult Success(Guid reportId) =>
        new(true, reportId, []);

    public static DailyReportActionResult Failure(params string[] errors) =>
        new(false, null, errors);
}
