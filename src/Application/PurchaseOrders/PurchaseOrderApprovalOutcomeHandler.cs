using Application.Approvals;
using Domain.Approvals;
using Domain.PurchaseOrders;

namespace Application.PurchaseOrders;

public sealed class PurchaseOrderApprovalOutcomeHandler(
    IPurchaseOrderRepository purchaseOrders)
    : IApprovalSubjectOutcomeHandler
{
    public string SubjectType =>
        PurchaseOrderApproval.SubjectType;

    public async Task ApplyAsync(
        ApprovalRequest approvalRequest,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            approvalRequest.SubjectId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            throw new InvalidOperationException(
                "Purchase order linked to this approval was not found.");
        }

        var outcome = approvalRequest.Status switch
        {
            ApprovalRequestStatus.Approved =>
                PurchaseOrderStatus.Approved,
            ApprovalRequestStatus.Rejected =>
                PurchaseOrderStatus.Rejected,
            ApprovalRequestStatus.Cancelled =>
                PurchaseOrderStatus.Draft,
            ApprovalRequestStatus.Pending =>
                (PurchaseOrderStatus?)null,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (outcome is not null)
        {
            purchaseOrder.ApplyApprovalOutcome(
                approvalRequest.Id,
                outcome.Value);
        }
    }
}
