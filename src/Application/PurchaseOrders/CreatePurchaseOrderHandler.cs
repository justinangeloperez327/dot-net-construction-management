using Application.Common.Authentication;
using Application.Projects;
using Application.PurchaseRequests;
using Application.Suppliers;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;

namespace Application.PurchaseOrders;

public sealed record CreatePurchaseOrderCommand(
    Guid PurchaseRequestId,
    Guid SupplierId,
    string PurchaseOrderNumber,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string Currency,
    string? DeliveryAddress,
    string? DeliveryTerms,
    string? PaymentTerms,
    string? Notes);

public sealed class CreatePurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrders,
    IPurchaseRequestRepository purchaseRequests,
    ISupplierRepository suppliers,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<PurchaseOrderActionResult> HandleAsync(
        CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await purchaseRequests.GetByIdAsync(
            command.PurchaseRequestId,
            cancellationToken);

        if (purchaseRequest is null)
        {
            return PurchaseOrderActionResult.Failure(
                "Purchase request was not found.");
        }

        if (purchaseRequest.Status != PurchaseRequestStatus.Approved)
        {
            return PurchaseOrderActionResult.Failure(
                "Only approved purchase requests can be converted to purchase orders.");
        }

        var project = await projects.GetByIdAsync(
            purchaseRequest.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return PurchaseOrderActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return PurchaseOrderActionResult.Failure(
                "Purchase orders cannot be created for a closed project.");
        }

        var supplier = await suppliers.GetByIdAsync(
            command.SupplierId,
            cancellationToken);

        if (supplier is null || !supplier.IsActive)
        {
            return PurchaseOrderActionResult.Failure(
                "An active supplier is required.");
        }

        var purchaseOrderNumber = command.PurchaseOrderNumber.Trim();

        if (await purchaseOrders.PurchaseOrderNumberExistsAsync(
                purchaseOrderNumber,
                cancellationToken: cancellationToken))
        {
            return PurchaseOrderActionResult.Failure(
                "A purchase order with this number already exists.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return PurchaseOrderActionResult.Failure(
                "An authenticated user is required.");
        }

        try
        {
            var purchaseOrder = PurchaseOrder.Create(
                purchaseRequest.ProjectId,
                purchaseRequest.Id,
                supplier.Id,
                purchaseOrderNumber,
                command.OrderDate,
                command.ExpectedDeliveryDate,
                command.Currency,
                command.DeliveryAddress,
                command.DeliveryTerms,
                command.PaymentTerms,
                command.Notes,
                userId,
                timeProvider.GetUtcNow());

            foreach (var sourceItem in purchaseRequest.Items)
            {
                purchaseOrder.AddItem(
                    sourceItem.Id,
                    sourceItem.Description,
                    sourceItem.Quantity,
                    sourceItem.Unit,
                    unitPrice: 0m,
                    discountPercent: 0m,
                    taxPercent: 0m,
                    sourceItem.Remarks);
            }

            await purchaseOrders.AddAsync(
                purchaseOrder,
                cancellationToken);

            await purchaseOrders.SaveChangesAsync(cancellationToken);

            return PurchaseOrderActionResult.Success(
                purchaseOrder.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PurchaseOrderActionResult.Failure(
                exception.Message);
        }
    }
}
