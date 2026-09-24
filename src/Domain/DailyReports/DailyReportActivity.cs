namespace Domain.DailyReports;

public sealed class DailyReportActivity
{
    private DailyReportActivity()
    {
    }

    internal DailyReportActivity(
        Guid dailyReportId,
        string workArea,
        string activity,
        DailyActivityStatus status,
        decimal? progressPercent,
        string? remarks)
    {
        Id = Guid.NewGuid();
        DailyReportId = dailyReportId;

        Update(
            workArea,
            activity,
            status,
            progressPercent,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid DailyReportId { get; private set; }

    public string WorkArea { get; private set; } = string.Empty;

    public string Activity { get; private set; } = string.Empty;

    public DailyActivityStatus Status { get; private set; }

    public decimal? ProgressPercent { get; private set; }

    public string? Remarks { get; private set; }

    internal void Update(
        string workArea,
        string activity,
        DailyActivityStatus status,
        decimal? progressPercent,
        string? remarks)
    {
        workArea = workArea.Trim();
        activity = activity.Trim();

        if (string.IsNullOrWhiteSpace(workArea))
        {
            throw new ArgumentException(
                "Work area is required.",
                nameof(workArea));
        }

        if (string.IsNullOrWhiteSpace(activity))
        {
            throw new ArgumentException(
                "Activity is required.",
                nameof(activity));
        }

        if (progressPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(progressPercent),
                "Progress percent must be between 0 and 100.");
        }

        WorkArea = workArea;
        Activity = activity;
        Status = status;
        ProgressPercent = progressPercent;
        Remarks = NormalizeOptional(remarks);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
