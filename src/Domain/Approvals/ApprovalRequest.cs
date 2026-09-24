namespace Domain.Approvals;

public sealed class ApprovalRequest
{
    private readonly List<ApprovalStep> _steps = [];

    private ApprovalRequest()
    {
    }

    private ApprovalRequest(
        Guid? projectId,
        string subjectType,
        Guid subjectId,
        string reference,
        string title,
        string? description,
        Guid requestedByUserId,
        DateTimeOffset requestedAt,
        IReadOnlyList<ApprovalStepAssignment> steps)
    {
        if (subjectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Subject ID is required.",
                nameof(subjectId));
        }

        if (requestedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Requested-by user ID is required.",
                nameof(requestedByUserId));
        }

        subjectType = subjectType.Trim();
        reference = reference.Trim();
        title = title.Trim();

        if (string.IsNullOrWhiteSpace(subjectType))
        {
            throw new ArgumentException(
                "Subject type is required.",
                nameof(subjectType));
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException(
                "Reference is required.",
                nameof(reference));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Approval title is required.",
                nameof(title));
        }

        if (steps.Count == 0)
        {
            throw new ArgumentException(
                "At least one approval step is required.",
                nameof(steps));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        SubjectType = subjectType;
        SubjectId = subjectId;
        Reference = reference;
        Title = title;
        Description = NormalizeOptional(description);
        RequestedByUserId = requestedByUserId;
        RequestedAt = requestedAt;
        Status = ApprovalRequestStatus.Pending;

        for (var index = 0; index < steps.Count; index++)
        {
            var assignment = steps[index];

            _steps.Add(
                new ApprovalStep(
                    Id,
                    index + 1,
                    assignment.Name,
                    assignment.ApproverUserId,
                    isFirstStep: index == 0));
        }
    }

    public Guid Id { get; private set; }

    public Guid? ProjectId { get; private set; }

    public string SubjectType { get; private set; } = string.Empty;

    public Guid SubjectId { get; private set; }

    public string Reference { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ApprovalRequestStatus Status { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<ApprovalStep> Steps => _steps;

    public ApprovalStep? CurrentStep =>
        _steps.SingleOrDefault(
            step => step.Status == ApprovalStepStatus.Pending);

    public static ApprovalRequest Create(
        Guid? projectId,
        string subjectType,
        Guid subjectId,
        string reference,
        string title,
        string? description,
        Guid requestedByUserId,
        DateTimeOffset requestedAt,
        IReadOnlyList<ApprovalStepAssignment> steps)
    {
        return new ApprovalRequest(
            projectId,
            subjectType,
            subjectId,
            reference,
            title,
            description,
            requestedByUserId,
            requestedAt,
            steps);
    }

    public void Decide(
        Guid stepId,
        Guid userId,
        ApprovalStepDecision decision,
        DateTimeOffset decidedAt,
        string? comments)
    {
        EnsurePending();

        var step = _steps.SingleOrDefault(
                item => item.Id == stepId)
            ?? throw new InvalidOperationException(
                "Approval step was not found.");

        if (decision == ApprovalStepDecision.Approve)
        {
            step.Approve(
                userId,
                decidedAt,
                comments);

            var nextStep = _steps
                .Where(item =>
                    item.StepNumber > step.StepNumber &&
                    item.Status == ApprovalStepStatus.Waiting)
                .OrderBy(item => item.StepNumber)
                .FirstOrDefault();

            if (nextStep is null)
            {
                Status = ApprovalRequestStatus.Approved;
                CompletedAt = decidedAt;
            }
            else
            {
                nextStep.Activate();
            }

            return;
        }

        step.Reject(
            userId,
            decidedAt,
            comments ?? string.Empty);

        foreach (var remaining in _steps.Where(
                     item => item.StepNumber > step.StepNumber))
        {
            remaining.Cancel();
        }

        Status = ApprovalRequestStatus.Rejected;
        CompletedAt = decidedAt;
    }

    public void Cancel(
        Guid requestedByUserId,
        DateTimeOffset cancelledAt,
        string? reason)
    {
        EnsurePending();

        if (requestedByUserId != RequestedByUserId)
        {
            throw new InvalidOperationException(
                "Only the requester can cancel this approval request.");
        }

        foreach (var step in _steps)
        {
            step.Cancel();
        }

        Status = ApprovalRequestStatus.Cancelled;
        CancelledAt = cancelledAt;
        CompletedAt = cancelledAt;
        CancellationReason = NormalizeOptional(reason);
    }

    private void EnsurePending()
    {
        if (Status != ApprovalRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending approval requests can be changed.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
