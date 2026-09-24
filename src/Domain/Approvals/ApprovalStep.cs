namespace Domain.Approvals;

public sealed class ApprovalStep
{
    private ApprovalStep()
    {
    }

    internal ApprovalStep(
        Guid approvalRequestId,
        int stepNumber,
        string name,
        Guid approverUserId,
        bool isFirstStep)
    {
        if (approvalRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Approval request ID is required.",
                nameof(approvalRequestId));
        }

        if (stepNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stepNumber),
                "Step number must be greater than zero.");
        }

        name = name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Approval step name is required.",
                nameof(name));
        }

        if (approverUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Approver user ID is required.",
                nameof(approverUserId));
        }

        Id = Guid.NewGuid();
        ApprovalRequestId = approvalRequestId;
        StepNumber = stepNumber;
        Name = name;
        ApproverUserId = approverUserId;
        Status = isFirstStep
            ? ApprovalStepStatus.Pending
            : ApprovalStepStatus.Waiting;
    }

    public Guid Id { get; private set; }

    public Guid ApprovalRequestId { get; private set; }

    public int StepNumber { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid ApproverUserId { get; private set; }

    public ApprovalStepStatus Status { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }

    public string? Comments { get; private set; }

    internal void Activate()
    {
        if (Status != ApprovalStepStatus.Waiting)
        {
            throw new InvalidOperationException(
                "Only waiting approval steps can be activated.");
        }

        Status = ApprovalStepStatus.Pending;
    }

    internal void Approve(
        Guid userId,
        DateTimeOffset decidedAt,
        string? comments)
    {
        EnsureCanDecide(userId);

        Status = ApprovalStepStatus.Approved;
        DecidedAt = decidedAt;
        Comments = NormalizeOptional(comments);
    }

    internal void Reject(
        Guid userId,
        DateTimeOffset decidedAt,
        string comments)
    {
        EnsureCanDecide(userId);

        comments = comments.Trim();

        if (string.IsNullOrWhiteSpace(comments))
        {
            throw new ArgumentException(
                "Comments are required when rejecting an approval.",
                nameof(comments));
        }

        Status = ApprovalStepStatus.Rejected;
        DecidedAt = decidedAt;
        Comments = comments;
    }

    internal void Cancel()
    {
        if (Status is ApprovalStepStatus.Waiting
            or ApprovalStepStatus.Pending)
        {
            Status = ApprovalStepStatus.Cancelled;
        }
    }

    private void EnsureCanDecide(Guid userId)
    {
        if (Status != ApprovalStepStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only the current pending approval step can be decided.");
        }

        if (userId != ApproverUserId)
        {
            throw new InvalidOperationException(
                "This approval step is assigned to another user.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
