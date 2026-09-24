using Domain.Approvals;

namespace Application.Approvals;

public interface IApprovalSubjectOutcomeHandler
{
    string SubjectType { get; }

    Task ApplyAsync(
        ApprovalRequest approvalRequest,
        CancellationToken cancellationToken = default);
}
