using Application.Projects;

namespace Application.PurchaseOrders;

public sealed class CancelPurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrders,
    IProjectRepository projects)
{
    public async Task<PurchaseOrderActionResult> HandleAsync(
        Guid purchaseOrderId,
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
            purchaseOrder.Cancel();

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
