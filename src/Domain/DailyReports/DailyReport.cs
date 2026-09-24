namespace Domain.DailyReports;

public sealed class DailyReport
{
    private readonly List<DailyReportActivity> _activities = [];
    private readonly List<DailyReportManpowerEntry> _manpowerEntries = [];
    private readonly List<DailyReportEquipmentEntry> _equipmentEntries = [];
    private readonly List<DailyReportSiteIssue> _siteIssues = [];

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

    public IReadOnlyCollection<DailyReportManpowerEntry> ManpowerEntries =>
        _manpowerEntries;

    public IReadOnlyCollection<DailyReportEquipmentEntry> EquipmentEntries =>
        _equipmentEntries;

    public IReadOnlyCollection<DailyReportSiteIssue> SiteIssues =>
        _siteIssues;

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

        FindActivity(activityId).Update(
            workArea,
            activity,
            status,
            progressPercent,
            remarks);
    }

    public void RemoveActivity(Guid activityId)
    {
        EnsureEditable();
        _activities.Remove(FindActivity(activityId));
    }

    public DailyReportManpowerEntry AddManpower(
        string trade,
        string? contractor,
        int headcount,
        decimal? manHours,
        string? remarks)
    {
        EnsureEditable();

        var item = new DailyReportManpowerEntry(
            Id,
            trade,
            contractor,
            headcount,
            manHours,
            remarks);

        _manpowerEntries.Add(item);

        return item;
    }

    public void UpdateManpower(
        Guid entryId,
        string trade,
        string? contractor,
        int headcount,
        decimal? manHours,
        string? remarks)
    {
        EnsureEditable();

        FindManpower(entryId).Update(
            trade,
            contractor,
            headcount,
            manHours,
            remarks);
    }

    public void RemoveManpower(Guid entryId)
    {
        EnsureEditable();
        _manpowerEntries.Remove(FindManpower(entryId));
    }

    public DailyReportEquipmentEntry AddEquipment(
        string equipment,
        string? identifier,
        int quantity,
        decimal? hoursUsed,
        string? remarks)
    {
        EnsureEditable();

        var item = new DailyReportEquipmentEntry(
            Id,
            equipment,
            identifier,
            quantity,
            hoursUsed,
            remarks);

        _equipmentEntries.Add(item);

        return item;
    }

    public void UpdateEquipment(
        Guid entryId,
        string equipment,
        string? identifier,
        int quantity,
        decimal? hoursUsed,
        string? remarks)
    {
        EnsureEditable();

        FindEquipment(entryId).Update(
            equipment,
            identifier,
            quantity,
            hoursUsed,
            remarks);
    }

    public void RemoveEquipment(Guid entryId)
    {
        EnsureEditable();
        _equipmentEntries.Remove(FindEquipment(entryId));
    }

    public DailyReportSiteIssue AddSiteIssue(
        string title,
        string description,
        string? actionTaken,
        SiteIssueStatus status)
    {
        EnsureEditable();

        var item = new DailyReportSiteIssue(
            Id,
            title,
            description,
            actionTaken,
            status);

        _siteIssues.Add(item);

        return item;
    }

    public void UpdateSiteIssue(
        Guid issueId,
        string title,
        string description,
        string? actionTaken,
        SiteIssueStatus status)
    {
        EnsureEditable();

        FindSiteIssue(issueId).Update(
            title,
            description,
            actionTaken,
            status);
    }

    public void RemoveSiteIssue(Guid issueId)
    {
        EnsureEditable();
        _siteIssues.Remove(FindSiteIssue(issueId));
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

    private DailyReportActivity FindActivity(Guid activityId) =>
        _activities.SingleOrDefault(activity => activity.Id == activityId)
        ?? throw new InvalidOperationException(
            "Daily report activity was not found.");

    private DailyReportManpowerEntry FindManpower(Guid entryId) =>
        _manpowerEntries.SingleOrDefault(entry => entry.Id == entryId)
        ?? throw new InvalidOperationException(
            "Manpower entry was not found.");

    private DailyReportEquipmentEntry FindEquipment(Guid entryId) =>
        _equipmentEntries.SingleOrDefault(entry => entry.Id == entryId)
        ?? throw new InvalidOperationException(
            "Equipment entry was not found.");

    private DailyReportSiteIssue FindSiteIssue(Guid issueId) =>
        _siteIssues.SingleOrDefault(issue => issue.Id == issueId)
        ?? throw new InvalidOperationException(
            "Site issue was not found.");

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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
