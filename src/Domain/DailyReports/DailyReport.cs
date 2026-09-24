namespace Domain.DailyReports;

public sealed class DailyReport
{
    private readonly List<DailyReportActivity> _activities = [];

    private DailyReport()
    {
    }

    private DailyReport(
        Guid projectId,
        DateOnly reportDate,
        Guid preparedByUserId,
        string? weather,
        string? remarks)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        if (preparedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Prepared-by user ID is required.",
                nameof(preparedByUserId));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        ReportDate = reportDate;
        PreparedByUserId = preparedByUserId;
        Status = DailyReportStatus.Draft;

        UpdateHeader(weather, remarks);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public DateOnly ReportDate { get; private set; }

    public Guid PreparedByUserId { get; private set; }

    public string? Weather { get; private set; }

    public string? Remarks { get; private set; }

    public DailyReportStatus Status { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? ReviewComments { get; private set; }

    public IReadOnlyCollection<DailyReportActivity> Activities => _activities;

    public static DailyReport Create(
        Guid projectId,
        DateOnly reportDate,
        Guid preparedByUserId,
        string? weather,
        string? remarks)
    {
        return new DailyReport(
            projectId,
            reportDate,
            preparedByUserId,
            weather,
            remarks);
    }

    public void UpdateHeader(
        string? weather,
        string? remarks)
    {
        EnsureEditable();

        Weather = NormalizeOptional(weather);
        Remarks = NormalizeOptional(remarks);
    }

    public DailyReportActivity AddActivity(
        string workArea,
        string activity,
        DailyActivityStatus status,
        decimal? progressPercent,
        string? remarks)
    {
        EnsureEditable();

        var item = new DailyReportActivity(
            Id,
            workArea,
            activity,
            status,
            progressPercent,
            remarks);

        _activities.Add(item);

        return item;
    }

    public void UpdateActivity(
        Guid activityId,
        string workArea,
        string activity,
        DailyActivityStatus status,
        decimal? progressPercent,
        string? remarks)
    {
        EnsureEditable();

        var item = FindActivity(activityId);

        item.Update(
            workArea,
            activity,
            status,
            progressPercent,
            remarks);
    }

    public void RemoveActivity(Guid activityId)
    {
        EnsureEditable();

        var item = FindActivity(activityId);
        _activities.Remove(item);
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        EnsureEditable();

        if (_activities.Count == 0)
        {
            throw new InvalidOperationException(
                "Add at least one activity before submitting the report.");
        }

        Status = DailyReportStatus.Submitted;
        SubmittedAt = submittedAt;
        ReviewedByUserId = null;
        ReviewedAt = null;
        ReviewComments = null;
    }

    public void Approve(
        Guid reviewerUserId,
        DateTimeOffset reviewedAt,
        string? comments)
    {
        EnsureSubmitted();
        EnsureReviewer(reviewerUserId);

        Status = DailyReportStatus.Approved;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = reviewedAt;
        ReviewComments = NormalizeOptional(comments);
    }

    public void Reject(
        Guid reviewerUserId,
        DateTimeOffset reviewedAt,
        string comments)
    {
        EnsureSubmitted();
        EnsureReviewer(reviewerUserId);

        comments = comments.Trim();

        if (string.IsNullOrWhiteSpace(comments))
        {
            throw new ArgumentException(
                "Review comments are required when rejecting a report.",
                nameof(comments));
        }

        Status = DailyReportStatus.Rejected;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = reviewedAt;
        ReviewComments = comments;
    }

    private DailyReportActivity FindActivity(Guid activityId)
    {
        return _activities.SingleOrDefault(
                activity => activity.Id == activityId)
            ?? throw new InvalidOperationException(
                "Daily report activity was not found.");
    }

    private void EnsureEditable()
    {
        if (Status is not DailyReportStatus.Draft and
            not DailyReportStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected reports can be edited.");
        }
    }

    private void EnsureSubmitted()
    {
        if (Status != DailyReportStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted reports can be reviewed.");
        }
    }

    private static void EnsureReviewer(Guid reviewerUserId)
    {
        if (reviewerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Reviewer user ID is required.",
                nameof(reviewerUserId));
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
