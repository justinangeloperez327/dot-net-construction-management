using Domain.Approvals;

namespace Application.Approvals;

public sealed record ApprovalStepInput(
    string Name,
    Guid ApproverUserId);

public sealed record ApprovalStepDetails(
    Guid Id,
    int StepNumber,
    string Name,
    Guid ApproverUserId,
    string Approver,
    ApprovalStepStatus Status,
    DateTimeOffset? DecidedAt,
    string? Comments);

public sealed record ApprovalRequestSummary(
    Guid Id,
    Guid? ProjectId,
    string SubjectType,
    Guid SubjectId,
    string Reference,
    string Title,
    ApprovalRequestStatus Status,
    string RequestedBy,
    DateTimeOffset RequestedAt,
    string? CurrentStepName,
    bool NeedsMyDecision);

public sealed record ApprovalRequestDetails(
    Guid Id,
    Guid? ProjectId,
    string SubjectType,
    Guid SubjectId,
    string Reference,
    string Title,
    string? Description,
    ApprovalRequestStatus Status,
    string RequestedBy,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    IReadOnlyList<ApprovalStepDetails> Steps,
    bool CanDecideCurrentStep,
    bool CanCancel);

public sealed record ApprovalRequestListResult(
    IReadOnlyList<ApprovalRequestSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record ApprovalActionResult(
    bool Succeeded,
    Guid? ApprovalRequestId,
    IReadOnlyList<string> Errors)
{
    public static ApprovalActionResult Success(Guid approvalRequestId) =>
        new(true, approvalRequestId, []);

    public static ApprovalActionResult Failure(params string[] errors) =>
        new(false, null, errors);
}
