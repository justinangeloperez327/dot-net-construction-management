using Application.Projects;
using Application.PurchaseOrders;

namespace Application.Deliveries;

public sealed record DeliveryItemCommand(
    Guid PurchaseOrderItemId,
    decimal Quantity,
    string? Remarks);

public sealed class AddDeliveryItemHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects)
{
    public async Task<DeliveryActionResult> HandleAsync(
        Guid deliveryId,
        DeliveryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var delivery = await deliveries.GetByIdAsync(
            deliveryId,
            cancellationToken);

        if (delivery is null)
        {
            return DeliveryActionResult.Failure(
                "Delivery was not found.");
        }

        var projectError = await DeliveryProjectGuard.ValidateActiveAsync(
            projects,
            delivery.ProjectId,
            cancellationToken);

        if (projectError is not null)
        {
            return DeliveryActionResult.Failure(projectError);
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            delivery.PurchaseOrderId,
            cancellationToken);

        var sourceItem = purchaseOrder?.Items.SingleOrDefault(
            item => item.Id == command.PurchaseOrderItemId);

        if (purchaseOrder is null ||
            sourceItem is null)
        {
            return DeliveryActionResult.Failure(
                "Purchase order item was not found.");
        }

        var quantityError = await ValidateQuantityAsync(
            deliveries,
            delivery.Id,
            sourceItem.Id,
            sourceItem.Quantity,
            command.Quantity,
            cancellationToken);

        if (quantityError is not null)
        {
            return DeliveryActionResult.Failure(quantityError);
        }

        try
        {
            delivery.AddItem(
                sourceItem.Id,
                sourceItem.Description,
                command.Quantity,
                sourceItem.Unit,
                command.Remarks);

            await deliveries.SaveChangesAsync(cancellationToken);

            return DeliveryActionResult.Success(delivery.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return DeliveryActionResult.Failure(exception.Message);
        }
    }

    internal static async Task<string?> ValidateQuantityAsync(
        IDeliveryRepository deliveries,
        Guid deliveryId,
        Guid purchaseOrderItemId,
        decimal orderedQuantity,
        decimal deliveryQuantity,
        CancellationToken cancellationToken)
    {
        if (deliveryQuantity <= 0)
        {
            return "Delivered quantity must be greater than zero.";
        }

        var alreadyReceived =
            await deliveries.GetReceivedQuantityForPurchaseOrderItemAsync(
                purchaseOrderItemId,
                deliveryId,
                cancellationToken);

        if (alreadyReceived + deliveryQuantity > orderedQuantity)
        {
            return "Delivered quantity exceeds the remaining purchase order quantity.";
        }

        return null;
    }
}

public sealed class UpdateDeliveryItemHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects)
{
    public async Task<DeliveryActionResult> HandleAsync(
        Guid deliveryId,
        Guid itemId,
        decimal quantity,
        string? remarks,
        CancellationToken cancellationToken = default)
    {
        var delivery = await deliveries.GetByIdAsync(
            deliveryId,
            cancellationToken);

        if (delivery is null)
        {
            return DeliveryActionResult.Failure(
                "Delivery was not found.");
        }

        var projectError = await DeliveryProjectGuard.ValidateActiveAsync(
            projects,
            delivery.ProjectId,
            cancellationToken);

        if (projectError is not null)
        {
            return DeliveryActionResult.Failure(projectError);
        }

        var deliveryItem = delivery.Items.SingleOrDefault(
            item => item.Id == itemId);

        if (deliveryItem is null)
        {
            return DeliveryActionResult.Failure(
                "Delivery item was not found.");
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            delivery.PurchaseOrderId,
            cancellationToken);

        var sourceItem = purchaseOrder?.Items.SingleOrDefault(
            item => item.Id == deliveryItem.PurchaseOrderItemId);

        if (sourceItem is null)
        {
            return DeliveryActionResult.Failure(
                "Purchase order item was not found.");
        }

        var quantityError =
            await AddDeliveryItemHandler.ValidateQuantityAsync(
                deliveries,
                delivery.Id,
                sourceItem.Id,
                sourceItem.Quantity,
                quantity,
                cancellationToken);

        if (quantityError is not null)
        {
            return DeliveryActionResult.Failure(quantityError);
        }

        try
        {
            delivery.UpdateItem(
                itemId,
                quantity,
                remarks);

            await deliveries.SaveChangesAsync(cancellationToken);

            return DeliveryActionResult.Success(delivery.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return DeliveryActionResult.Failure(exception.Message);
        }
    }
}

public sealed class RemoveDeliveryItemHandler(
    IDeliveryRepository deliveries,
    IProjectRepository projects)
{
    public async Task<DeliveryActionResult> HandleAsync(
        Guid deliveryId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await deliveries.GetByIdAsync(
            deliveryId,
            cancellationToken);

        if (delivery is null)
        {
            return DeliveryActionResult.Failure(
                "Delivery was not found.");
        }

        var projectError = await DeliveryProjectGuard.ValidateActiveAsync(
            projects,
            delivery.ProjectId,
            cancellationToken);

        if (projectError is not null)
        {
            return DeliveryActionResult.Failure(projectError);
        }

        try
        {
            delivery.RemoveItem(itemId);

            await deliveries.SaveChangesAsync(cancellationToken);

            return DeliveryActionResult.Success(delivery.Id);
        }
        catch (InvalidOperationException exception)
        {
            return DeliveryActionResult.Failure(exception.Message);
        }
    }
}
