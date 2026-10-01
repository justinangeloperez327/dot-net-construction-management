using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.Projects;
using Application.PurchaseRequests;
using Application.Suppliers;
using Application.Users;
using Domain.Approvals;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;

namespace Application.PurchaseOrders;

public sealed record SubmitPurchaseOrderCommand(
    IReadOnlyList<ApprovalStepInput> Steps);

public sealed class SubmitPurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrders,
    IPurchaseRequestRepository purchaseRequests,
    IApprovalRequestRepository approvals,
    ISupplierRepository suppliers,
    IProjectRepository projects,
    IUserDirectory users,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<PurchaseOrderActionResult> HandleAsync(
        Guid purchaseOrderId,
        SubmitPurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            purchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return PurchaseOrderActionResult.Failure(
                "Purchase order was not found.");
        }

        var project = await projects.GetByIdAsync(
            purchaseOrder.ProjectId,
            cancellationToken);

        if (project is null ||
            project.Status == ProjectStatus.Closed)
        {
            return PurchaseOrderActionResult.Failure(
                "Purchase orders in a closed or missing project cannot be submitted.");
        }

        var purchaseRequest = await purchaseRequests.GetByIdAsync(
            purchaseOrder.PurchaseRequestId,
            cancellationToken);

        if (purchaseRequest is null ||
            purchaseRequest.Status != PurchaseRequestStatus.Approved)
        {
            return PurchaseOrderActionResult.Failure(
                "The source purchase request must remain approved.");
        }

        var allocationError = await ValidateSourceQuantitiesAsync(
            purchaseOrder,
            purchaseRequest,
            cancellationToken);

        if (allocationError is not null)
        {
            return PurchaseOrderActionResult.Failure(allocationError);
        }

        var supplier = await suppliers.GetByIdAsync(
            purchaseOrder.SupplierId,
            cancellationToken);

        if (supplier is null || !supplier.IsActive)
        {
            return PurchaseOrderActionResult.Failure(
                "An active supplier is required.");
        }

        if (command.Steps.Count == 0)
        {
            return PurchaseOrderActionResult.Failure(
                "At least one approval step is required.");
        }

        if (await approvals.HasPendingForSubjectAsync(
                PurchaseOrderApproval.SubjectType,
                purchaseOrder.Id,
                cancellationToken))
        {
            return PurchaseOrderActionResult.Failure(
                "This purchase order already has a pending approval.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return PurchaseOrderActionResult.Failure(
                "An authenticated user is required.");
        }

        if (purchaseOrder.CreatedByUserId != userId)
        {
            return PurchaseOrderActionResult.Failure(
                "Only the purchase order creator can submit it for approval.");
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
            return PurchaseOrderActionResult.Failure(
                "Every approval step must be assigned to an active user.");
        }

        try
        {
            var approval = ApprovalRequest.Create(
                purchaseOrder.ProjectId,
                PurchaseOrderApproval.SubjectType,
                purchaseOrder.Id,
                purchaseOrder.PurchaseOrderNumber,
                $"Purchase order: {purchaseOrder.PurchaseOrderNumber}",
                $"Supplier: {supplier.Name}; Total: {purchaseOrder.Currency} {purchaseOrder.GrandTotal:N2}",
                userId,
                timeProvider.GetUtcNow(),
                command.Steps
                    .Select(step => new ApprovalStepAssignment(
                        step.Name,
                        step.ApproverUserId))
                    .ToArray());

            purchaseOrder.SubmitForApproval(approval.Id);

            await approvals.AddAsync(
                approval,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return PurchaseOrderActionResult.Success(
                purchaseOrder.Id,
                approval.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PurchaseOrderActionResult.Failure(
                exception.Message);
        }
    }

    private async Task<string?> ValidateSourceQuantitiesAsync(
        PurchaseOrder purchaseOrder,
        PurchaseRequest purchaseRequest,
        CancellationToken cancellationToken)
    {
        foreach (var item in purchaseOrder.Items)
        {
            if (item.PurchaseRequestItemId is not Guid sourceItemId)
            {
                continue;
            }

            var sourceItem = purchaseRequest.Items.SingleOrDefault(
                candidate => candidate.Id == sourceItemId);

            if (sourceItem is null)
            {
                return "A source purchase request item could not be found.";
            }

            var committedQuantity =
                await purchaseOrders.GetCommittedQuantityForSourceItemAsync(
                    sourceItemId,
                    purchaseOrder.Id,
                    cancellationToken);

            if (committedQuantity + item.Quantity > sourceItem.Quantity)
            {
                return $"Item '{sourceItem.Description}' exceeds the remaining approved purchase request quantity.";
            }
        }

        return null;
    }
}
