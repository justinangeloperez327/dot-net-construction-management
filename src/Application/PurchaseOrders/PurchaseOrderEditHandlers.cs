using Application.Projects;
using Application.PurchaseRequests;
using Application.Suppliers;
using Domain.Projects;

namespace Application.PurchaseOrders;

internal static class PurchaseOrderProjectGuard
{
    public static async Task<string?> ValidateActiveAsync(
        IProjectRepository projects,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            return "Project was not found.";
        }

        return project.Status == ProjectStatus.Active
            ? null
            : "Purchase orders in a closed project cannot be changed.";
    }
}

public sealed record UpdatePurchaseOrderCommand(
    Guid SupplierId,
    string PurchaseOrderNumber,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string Currency,
    string? DeliveryAddress,
    string? DeliveryTerms,
    string? PaymentTerms,
    string? Notes);

public sealed class UpdatePurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    IProjectRepository projects)
{
    public async Task<PurchaseOrderActionResult> HandleAsync(
        Guid purchaseOrderId,
        UpdatePurchaseOrderCommand command,
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

        var projectError =
            await PurchaseOrderProjectGuard.ValidateActiveAsync(
                projects,
                purchaseOrder.ProjectId,
                cancellationToken);

        if (projectError is not null)
        {
            return PurchaseOrderActionResult.Failure(projectError);
        }

        var supplier = await suppliers.GetByIdAsync(
            command.SupplierId,
            cancellationToken);

        if (supplier is null || !supplier.IsActive)
        {
            return PurchaseOrderActionResult.Failure(
                "An active supplier is required.");
        }

        var number = command.PurchaseOrderNumber.Trim();

        if (await purchaseOrders.PurchaseOrderNumberExistsAsync(
                number,
                purchaseOrder.Id,
                cancellationToken))
        {
            return PurchaseOrderActionResult.Failure(
                "A purchase order with this number already exists.");
        }

        try
        {
            purchaseOrder.UpdateHeader(
                supplier.Id,
                number,
                command.OrderDate,
                command.ExpectedDeliveryDate,
                command.Currency,
                command.DeliveryAddress,
                command.DeliveryTerms,
                command.PaymentTerms,
                command.Notes);

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

public sealed record UpdatePurchaseOrderItemCommand(
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    string? Remarks);

public sealed class UpdatePurchaseOrderItemHandler(
    IPurchaseOrderRepository purchaseOrders,
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects)
{
    public async Task<PurchaseOrderActionResult> HandleAsync(
        Guid purchaseOrderId,
        Guid itemId,
        UpdatePurchaseOrderItemCommand command,
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

        var projectError =
            await PurchaseOrderProjectGuard.ValidateActiveAsync(
                projects,
                purchaseOrder.ProjectId,
                cancellationToken);

        if (projectError is not null)
        {
            return PurchaseOrderActionResult.Failure(projectError);
        }

        var item = purchaseOrder.Items.SingleOrDefault(
            entry => entry.Id == itemId);

        if (item is null)
        {
            return PurchaseOrderActionResult.Failure(
                "Purchase order item was not found.");
        }

        if (item.PurchaseRequestItemId is Guid sourceItemId)
        {
            var purchaseRequest = await purchaseRequests.GetByIdAsync(
                purchaseOrder.PurchaseRequestId,
                cancellationToken);

            var sourceItem = purchaseRequest?.Items.SingleOrDefault(
                entry => entry.Id == sourceItemId);

            if (sourceItem is null)
            {
                return PurchaseOrderActionResult.Failure(
                    "Source purchase request item was not found.");
            }

            if (command.Quantity > sourceItem.Quantity)
            {
                return PurchaseOrderActionResult.Failure(
                    "Purchase order quantity cannot exceed the source purchase request quantity.");
            }
        }

        try
        {
            purchaseOrder.UpdateItem(
                itemId,
                command.Quantity,
                command.UnitPrice,
                command.DiscountPercent,
                command.TaxPercent,
                command.Remarks);

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

public sealed class RemovePurchaseOrderItemHandler(
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects)
{
    public async Task<PurchaseOrderActionResult> HandleAsync(
        Guid purchaseOrderId,
        Guid itemId,
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

        var projectError =
            await PurchaseOrderProjectGuard.ValidateActiveAsync(
                projects,
                purchaseOrder.ProjectId,
                cancellationToken);

        if (projectError is not null)
        {
            return PurchaseOrderActionResult.Failure(projectError);
        }

        try
        {
            purchaseOrder.RemoveItem(itemId);

            await purchaseOrders.SaveChangesAsync(cancellationToken);

            return PurchaseOrderActionResult.Success(
                purchaseOrder.Id);
        }
        catch (InvalidOperationException exception)
        {
            return PurchaseOrderActionResult.Failure(
                exception.Message);
        }
    }
}
