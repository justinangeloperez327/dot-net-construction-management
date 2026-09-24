using Domain.Approvals;

namespace Application.Approvals;

public interface IApprovalRequestRepository
{
    Task<ApprovalRequest?> GetByIdAsync(
        Guid approvalRequestId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingForSubjectAsync(
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApprovalRequest>> ListForUserAsync(
        Guid userId,
        ApprovalRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountForUserAsync(
        Guid userId,
        ApprovalRequestStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
