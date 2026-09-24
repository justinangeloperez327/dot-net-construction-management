namespace Domain.DailyReports;

public sealed class DailyReportSiteIssue
{
    private DailyReportSiteIssue()
    {
    }

    internal DailyReportSiteIssue(
        Guid dailyReportId,
        string title,
        string description,
        string? actionTaken,
        SiteIssueStatus status)
    {
        Id = Guid.NewGuid();
        DailyReportId = dailyReportId;

        Update(
            title,
            description,
            actionTaken,
            status);
    }

    public Guid Id { get; private set; }

    public Guid DailyReportId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? ActionTaken { get; private set; }

    public SiteIssueStatus Status { get; private set; }

    internal void Update(
        string title,
        string description,
        string? actionTaken,
        SiteIssueStatus status)
    {
        title = title.Trim();
        description = description.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Issue title is required.",
                nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Issue description is required.",
                nameof(description));
        }

        Title = title;
        Description = description;
        ActionTaken = NormalizeOptional(actionTaken);
        Status = status;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
