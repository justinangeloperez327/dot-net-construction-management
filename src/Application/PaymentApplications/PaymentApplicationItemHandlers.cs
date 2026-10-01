using Application.Deliveries;
using Application.PurchaseOrders;

namespace Application.PaymentApplications;

public sealed record PaymentApplicationItemCommand(
    Guid PurchaseOrderItemId,
    decimal ClaimedQuantity,
    string? Remarks);

public sealed class AddPaymentApplicationItemHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    IDeliveryRepository deliveries)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        Guid paymentApplicationId,
        PaymentApplicationItemCommand command,
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

        var sourceItem = purchaseOrder?.Items.SingleOrDefault(
            item => item.Id == command.PurchaseOrderItemId);

        if (purchaseOrder is null || sourceItem is null)
        {
            return PaymentApplicationActionResult.Failure(
                "Purchase order item was not found.");
        }

        var quantityError = await ValidateQuantityAsync(
            paymentApplications,
            deliveries,
            paymentApplication.Id,
            sourceItem.Id,
            command.ClaimedQuantity,
            cancellationToken);

        if (quantityError is not null)
        {
            return PaymentApplicationActionResult.Failure(quantityError);
        }

        try
        {
            paymentApplication.AddItem(
                sourceItem.Id,
                sourceItem.Description,
                command.ClaimedQuantity,
                sourceItem.Unit,
                sourceItem.UnitPrice,
                sourceItem.DiscountPercent,
                sourceItem.TaxPercent,
                command.Remarks);

            await paymentApplications.SaveChangesAsync(cancellationToken);

            return PaymentApplicationActionResult.Success(
                paymentApplication.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PaymentApplicationActionResult.Failure(
                exception.Message);
        }
    }

    internal static async Task<string?> ValidateQuantityAsync(
        IPaymentApplicationRepository paymentApplications,
        IDeliveryRepository deliveries,
        Guid paymentApplicationId,
        Guid purchaseOrderItemId,
        decimal claimedQuantity,
        CancellationToken cancellationToken)
    {
        if (claimedQuantity <= 0)
        {
            return "Claimed quantity must be greater than zero.";
        }

        var receivedQuantity =
            await deliveries.GetReceivedQuantityForPurchaseOrderItemAsync(
                purchaseOrderItemId,
                cancellationToken: cancellationToken);

        var committedClaimQuantity =
            await paymentApplications
                .GetCommittedClaimQuantityForPurchaseOrderItemAsync(
                    purchaseOrderItemId,
                    paymentApplicationId,
                    cancellationToken);

        if (committedClaimQuantity + claimedQuantity > receivedQuantity)
        {
            return "Claimed quantity exceeds the received quantity currently available for payment.";
        }

        return null;
    }
}

public sealed class UpdatePaymentApplicationItemHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    IDeliveryRepository deliveries)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        Guid paymentApplicationId,
        Guid itemId,
        decimal claimedQuantity,
        string? remarks,
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

        var item = paymentApplication.Items.SingleOrDefault(
            candidate => candidate.Id == itemId);

        if (item is null)
        {
            return PaymentApplicationActionResult.Failure(
                "Payment application item was not found.");
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            paymentApplication.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder?.Items.All(
                candidate => candidate.Id != item.PurchaseOrderItemId)
            != false)
        {
            return PaymentApplicationActionResult.Failure(
                "Purchase order item was not found.");
        }

        var quantityError = await AddPaymentApplicationItemHandler
            .ValidateQuantityAsync(
                paymentApplications,
                deliveries,
                paymentApplication.Id,
                item.PurchaseOrderItemId,
                claimedQuantity,
                cancellationToken);

        if (quantityError is not null)
        {
            return PaymentApplicationActionResult.Failure(quantityError);
        }

        try
        {
            paymentApplication.UpdateItem(
                itemId,
                claimedQuantity,
                remarks);

            await paymentApplications.SaveChangesAsync(cancellationToken);

            return PaymentApplicationActionResult.Success(
                paymentApplication.Id);
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

public sealed class RemovePaymentApplicationItemHandler(
    IPaymentApplicationRepository paymentApplications)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        Guid paymentApplicationId,
        Guid itemId,
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

        try
        {
            paymentApplication.RemoveItem(itemId);

            await paymentApplications.SaveChangesAsync(cancellationToken);

            return PaymentApplicationActionResult.Success(
                paymentApplication.Id);
        }
        catch (InvalidOperationException exception)
        {
            return PaymentApplicationActionResult.Failure(
                exception.Message);
        }
    }
}
