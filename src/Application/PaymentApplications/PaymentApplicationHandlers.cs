using Application.Common.Authentication;
using Application.Deliveries;
using Application.Projects;
using Application.PurchaseOrders;
using Domain.PaymentApplications;
using Domain.PurchaseOrders;

namespace Application.PaymentApplications;

public sealed record CreatePaymentApplicationCommand(
    Guid PurchaseOrderId,
    string ApplicationNumber,
    DateOnly ApplicationDate,
    DateOnly? PeriodFrom,
    DateOnly? PeriodTo,
    string? Notes);

public sealed class CreatePaymentApplicationHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    IDeliveryRepository deliveries,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        CreatePaymentApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            command.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return PaymentApplicationActionResult.Failure(
                "Purchase order was not found.");
        }

        if (purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            return PaymentApplicationActionResult.Failure(
                "Payment applications can only be created against an approved purchase order.");
        }

        var project = await projects.GetByIdAsync(
            purchaseOrder.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return PaymentApplicationActionResult.Failure(
                "Project was not found.");
        }

        if (command.ApplicationDate < purchaseOrder.OrderDate)
        {
            return PaymentApplicationActionResult.Failure(
                "Payment application date cannot be before the purchase order date.");
        }

        var applicationNumber = command.ApplicationNumber.Trim();

        if (await paymentApplications.ApplicationNumberExistsAsync(
                purchaseOrder.Id,
                applicationNumber,
                cancellationToken: cancellationToken))
        {
            return PaymentApplicationActionResult.Failure(
                "A payment application with this number already exists for the purchase order.");
        }

        var hasClaimableQuantity = false;

        foreach (var orderItem in purchaseOrder.Items)
        {
            var receivedQuantity =
                await deliveries.GetReceivedQuantityForPurchaseOrderItemAsync(
                    orderItem.Id,
                    cancellationToken: cancellationToken);

            var committedClaimQuantity =
                await paymentApplications
                    .GetCommittedClaimQuantityForPurchaseOrderItemAsync(
                        orderItem.Id,
                        Guid.Empty,
                        cancellationToken);

            if (receivedQuantity > committedClaimQuantity)
            {
                hasClaimableQuantity = true;
                break;
            }
        }

        if (!hasClaimableQuantity)
        {
            return PaymentApplicationActionResult.Failure(
                "No received quantity is currently available to claim against this purchase order.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return PaymentApplicationActionResult.Failure(
                "An authenticated user is required.");
        }

        try
        {
            var paymentApplication = PaymentApplication.Create(
                purchaseOrder.ProjectId,
                purchaseOrder.Id,
                applicationNumber,
                command.ApplicationDate,
                command.PeriodFrom,
                command.PeriodTo,
                command.Notes,
                userId,
                timeProvider.GetUtcNow());

            await paymentApplications.AddAsync(
                paymentApplication,
                cancellationToken);

            await paymentApplications.SaveChangesAsync(cancellationToken);

            return PaymentApplicationActionResult.Success(
                paymentApplication.Id);
        }
        catch (ArgumentException exception)
        {
            return PaymentApplicationActionResult.Failure(
                exception.Message);
        }
    }
}

public sealed record UpdatePaymentApplicationCommand(
    string ApplicationNumber,
    DateOnly ApplicationDate,
    DateOnly? PeriodFrom,
    DateOnly? PeriodTo,
    string? Notes);

public sealed class UpdatePaymentApplicationHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders)
{
    public async Task<PaymentApplicationActionResult> HandleAsync(
        Guid paymentApplicationId,
        UpdatePaymentApplicationCommand command,
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

        if (command.ApplicationDate < purchaseOrder.OrderDate)
        {
            return PaymentApplicationActionResult.Failure(
                "Payment application date cannot be before the purchase order date.");
        }

        var applicationNumber = command.ApplicationNumber.Trim();

        if (await paymentApplications.ApplicationNumberExistsAsync(
                purchaseOrder.Id,
                applicationNumber,
                paymentApplication.Id,
                cancellationToken))
        {
            return PaymentApplicationActionResult.Failure(
                "A payment application with this number already exists for the purchase order.");
        }

        try
        {
            paymentApplication.UpdateHeader(
                applicationNumber,
                command.ApplicationDate,
                command.PeriodFrom,
                command.PeriodTo,
                command.Notes);

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
