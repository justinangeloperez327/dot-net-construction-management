using Application.Common.Authentication;
using Application.Projects;
using Application.PurchaseOrders;
using Domain.PurchaseOrders;

namespace Application.Deliveries;

public sealed class ReceiveDeliveryHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DeliveryActionResult> HandleAsync(
        Guid deliveryId,
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

        if (purchaseOrder is null ||
            purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            return DeliveryActionResult.Failure(
                "The purchase order must be approved before receiving a delivery.");
        }

        foreach (var item in delivery.Items)
        {
            var sourceItem = purchaseOrder.Items.SingleOrDefault(
                source => source.Id == item.PurchaseOrderItemId);

            if (sourceItem is null)
            {
                return DeliveryActionResult.Failure(
                    "A purchase order item linked to the delivery was not found.");
            }

            var alreadyReceived =
                await deliveries.GetReceivedQuantityForPurchaseOrderItemAsync(
                    item.PurchaseOrderItemId,
                    delivery.Id,
                    cancellationToken);

            if (alreadyReceived + item.Quantity > sourceItem.Quantity)
            {
                return DeliveryActionResult.Failure(
                    $"Item '{sourceItem.Description}' exceeds the remaining purchase order quantity.");
            }
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DeliveryActionResult.Failure(
                "An authenticated receiver is required.");
        }

        try
        {
            delivery.Receive(
                userId,
                timeProvider.GetUtcNow());

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

public sealed class CancelDeliveryHandler(
    IDeliveryRepository deliveries,
    IProjectRepository projects)
{
    public async Task<DeliveryActionResult> HandleAsync(
        Guid deliveryId,
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
            delivery.Cancel();

            await deliveries.SaveChangesAsync(cancellationToken);

            return DeliveryActionResult.Success(delivery.Id);
        }
        catch (InvalidOperationException exception)
        {
            return DeliveryActionResult.Failure(exception.Message);
        }
    }
}
