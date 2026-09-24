using Application.Approvals;
using Domain.Approvals;
using Domain.PurchaseRequests;

namespace Application.PurchaseRequests;

public sealed class PurchaseRequestApprovalOutcomeHandler(
    IPurchaseRequestRepository purchaseRequests)
    : IApprovalSubjectOutcomeHandler
{
    public string SubjectType =>
        PurchaseRequestApproval.SubjectType;

    public async Task ApplyAsync(
        ApprovalRequest approvalRequest,
        CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await purchaseRequests.GetByIdAsync(
            approvalRequest.SubjectId,
            cancellationToken);

        if (purchaseRequest is null)
        {
            throw new InvalidOperationException(
                "Purchase request linked to this approval was not found.");
        }

        var outcome = approvalRequest.Status switch
        {
            ApprovalRequestStatus.Approved =>
                PurchaseRequestStatus.Approved,
            ApprovalRequestStatus.Rejected =>
                PurchaseRequestStatus.Rejected,
            ApprovalRequestStatus.Cancelled =>
                PurchaseRequestStatus.Draft,
            ApprovalRequestStatus.Pending =>
                (PurchaseRequestStatus?)null,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (outcome is not null)
        {
            purchaseRequest.ApplyApprovalOutcome(
                approvalRequest.Id,
                outcome.Value);
        }
    }
}
