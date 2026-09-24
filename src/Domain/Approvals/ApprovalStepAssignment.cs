namespace Domain.Approvals;

public sealed record ApprovalStepAssignment(
    string Name,
    Guid ApproverUserId);
