using Application.Projects;

namespace Application.PurchaseRequests;

public sealed class CancelPurchaseRequestHandler(
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await purchaseRequests.GetByIdAsync(
            purchaseRequestId,
            cancellationToken);

        if (purchaseRequest is null)
        {
            return PurchaseRequestActionResult.Failure(
                "Purchase request was not found.");
        }

        var projectError =
            await PurchaseRequestProjectGuard.ValidateActiveAsync(
                projects,
                purchaseRequest.ProjectId,
                cancellationToken);

        if (projectError is not null)
        {
            return PurchaseRequestActionResult.Failure(projectError);
        }

        try
        {
            purchaseRequest.Cancel();

            await purchaseRequests.SaveChangesAsync(cancellationToken);

            return PurchaseRequestActionResult.Success(
                purchaseRequest.Id);
        }
        catch (InvalidOperationException exception)
        {
            return PurchaseRequestActionResult.Failure(
                exception.Message);
        }
    }
}
