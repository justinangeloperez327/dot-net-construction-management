using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.Deliveries;
using Application.PurchaseOrders;
using Application.Suppliers;
using Application.Users;
using Domain.Approvals;
using Domain.PurchaseOrders;

namespace Application.PaymentApplications;

public sealed record SubmitPaymentApplicationCommand(
    IReadOnlyList<ApprovalStepInput> Steps);

public sealed class SubmitPaymentApplicationHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    IDeliveryRepository deliveries,
    IApprovalRequestRepository approvals,
    ISupplierRepository suppliers,
    IUserDirectory users,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        Guid paymentApplicationId,
        SubmitPaymentApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var paymentApplication = await paymentApplications.GetByIdAsync(
            paymentApplicationId,
            cancellationToken);

        if (paymentApplication is null)
        {
            return PaymentApplicationActionResult.Failure(
                "Payment application was not found.");
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            paymentApplication.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null ||
            purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            return PaymentApplicationActionResult.Failure(
                "The purchase order must remain approved.");
        }

        foreach (var item in paymentApplication.Items)
        {
            var quantityError = await AddPaymentApplicationItemHandler
                .ValidateQuantityAsync(
                    paymentApplications,
                    deliveries,
                    paymentApplication.Id,
                    item.PurchaseOrderItemId,
                    item.ClaimedQuantity,
                    cancellationToken);

            if (quantityError is not null)
            {
                return PaymentApplicationActionResult.Failure(
                    $"Item '{item.Description}': {quantityError}");
            }
        }

        if (command.Steps.Count == 0)
        {
            return PaymentApplicationActionResult.Failure(
                "At least one approval step is required.");
        }

        if (await approvals.HasPendingForSubjectAsync(
                PaymentApplicationApproval.SubjectType,
                paymentApplication.Id,
                cancellationToken))
        {
            return PaymentApplicationActionResult.Failure(
                "This payment application already has a pending approval.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return PaymentApplicationActionResult.Failure(
                "An authenticated user is required.");
        }

        if (paymentApplication.CreatedByUserId != userId)
        {
            return PaymentApplicationActionResult.Failure(
                "Only the payment application creator can submit it for approval.");
        }

        var approverIds = command.Steps
            .Select(step => step.ApproverUserId)
            .Distinct()
            .ToArray();

        var approvers = await users.ListByIdsAsync(
            approverIds,
            cancellationToken);

        if (approvers.Count != approverIds.Length ||
            approvers.Any(approver => !approver.IsActive))
        {
            return PaymentApplicationActionResult.Failure(
                "Every approval step must be assigned to an active user.");
        }

        var supplier = await suppliers.GetByIdAsync(
            purchaseOrder.SupplierId,
            cancellationToken);

        try
        {
            var approval = ApprovalRequest.Create(
                paymentApplication.ProjectId,
                PaymentApplicationApproval.SubjectType,
                paymentApplication.Id,
                paymentApplication.ApplicationNumber,
                $"Payment application: {paymentApplication.ApplicationNumber}",
                $"PO: {purchaseOrder.PurchaseOrderNumber}; Supplier: {supplier?.Name ?? "Unknown supplier"}; Claimed: {purchaseOrder.Currency} {paymentApplication.ClaimedAmount:N2}",
                userId,
                timeProvider.GetUtcNow(),
                command.Steps
                    .Select(step => new ApprovalStepAssignment(
                        step.Name,
                        step.ApproverUserId))
                    .ToArray());

            paymentApplication.SubmitForApproval(approval.Id);

            await approvals.AddAsync(
                approval,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return PaymentApplicationActionResult.Success(
                paymentApplication.Id,
                approval.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PaymentApplicationActionResult.Failure(
                exception.Message);
        }
    }
}
