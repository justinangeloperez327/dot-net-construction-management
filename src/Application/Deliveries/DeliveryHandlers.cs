using Application.Common.Authentication;
using Application.Projects;
using Application.PurchaseOrders;
using Domain.Deliveries;
using Domain.Projects;
using Domain.PurchaseOrders;

namespace Application.Deliveries;

public sealed record CreateDeliveryCommand(
    Guid PurchaseOrderId,
    string DeliveryNoteNumber,
    DateOnly DeliveryDate,
    string? VehicleReference,
    string? Remarks);

public sealed class CreateDeliveryHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DeliveryActionResult> HandleAsync(
        CreateDeliveryCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            command.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return DeliveryActionResult.Failure(
                "Purchase order was not found.");
        }

        if (purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            return DeliveryActionResult.Failure(
                "Deliveries can only be recorded against an approved purchase order.");
        }

        var project = await projects.GetByIdAsync(
            purchaseOrder.ProjectId,
            cancellationToken);

        if (project is null ||
            project.Status == ProjectStatus.Closed)
        {
            return DeliveryActionResult.Failure(
                "Deliveries cannot be created for a closed or missing project.");
        }

        if (command.DeliveryDate < purchaseOrder.OrderDate)
        {
            return DeliveryActionResult.Failure(
                "Delivery date cannot be before the purchase order date.");
        }

        var deliveryNoteNumber = command.DeliveryNoteNumber.Trim();

        if (await deliveries.DeliveryNoteExistsAsync(
                purchaseOrder.Id,
                deliveryNoteNumber,
                cancellationToken: cancellationToken))
        {
            return DeliveryActionResult.Failure(
                "A delivery with this delivery note number already exists for the purchase order.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DeliveryActionResult.Failure(
                "An authenticated user is required.");
        }

        try
        {
            var delivery = Delivery.Create(
                purchaseOrder.ProjectId,
                purchaseOrder.Id,
                deliveryNoteNumber,
                command.DeliveryDate,
                command.VehicleReference,
                command.Remarks,
                userId,
                timeProvider.GetUtcNow());

            await deliveries.AddAsync(
                delivery,
                cancellationToken);

            await deliveries.SaveChangesAsync(cancellationToken);

            return DeliveryActionResult.Success(delivery.Id);
        }
        catch (ArgumentException exception)
        {
            return DeliveryActionResult.Failure(exception.Message);
        }
    }
}

public sealed record UpdateDeliveryCommand(
    string DeliveryNoteNumber,
    DateOnly DeliveryDate,
    string? VehicleReference,
    string? Remarks);

public sealed class UpdateDeliveryHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects)
{
    public async Task<DeliveryActionResult> HandleAsync(
        Guid deliveryId,
        UpdateDeliveryCommand command,
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
                "The purchase order must remain approved.");
        }

        if (command.DeliveryDate < purchaseOrder.OrderDate)
        {
            return DeliveryActionResult.Failure(
                "Delivery date cannot be before the purchase order date.");
        }

        var deliveryNoteNumber = command.DeliveryNoteNumber.Trim();

        if (await deliveries.DeliveryNoteExistsAsync(
                delivery.PurchaseOrderId,
                deliveryNoteNumber,
                delivery.Id,
                cancellationToken))
        {
            return DeliveryActionResult.Failure(
                "A delivery with this delivery note number already exists for the purchase order.");
        }

        try
        {
            delivery.UpdateHeader(
                deliveryNoteNumber,
                command.DeliveryDate,
                command.VehicleReference,
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
}

internal static class DeliveryProjectGuard
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
            : "Deliveries in a closed project cannot be changed.";
    }
}
