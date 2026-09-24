using Application.Common.Authentication;
using Application.Projects;
using Domain.Projects;
using Domain.PurchaseRequests;

namespace Application.PurchaseRequests;

public sealed record CreatePurchaseRequestCommand(
    Guid ProjectId,
    string RequestNumber,
    string Title,
    DateOnly? RequiredByDate,
    string? Purpose);

public sealed class CreatePurchaseRequestHandler(
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        CreatePurchaseRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            command.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return PurchaseRequestActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return PurchaseRequestActionResult.Failure(
                "Purchase requests cannot be created for a closed project.");
        }

        var requestNumber = command.RequestNumber.Trim();

        if (await purchaseRequests.RequestNumberExistsAsync(
                command.ProjectId,
                requestNumber,
                cancellationToken: cancellationToken))
        {
            return PurchaseRequestActionResult.Failure(
                "A purchase request with this number already exists in the project.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return PurchaseRequestActionResult.Failure(
                "An authenticated requester is required.");
        }

        try
        {
            var purchaseRequest = PurchaseRequest.Create(
                command.ProjectId,
                requestNumber,
                command.Title,
                command.RequiredByDate,
                command.Purpose,
                userId,
                timeProvider.GetUtcNow());

            await purchaseRequests.AddAsync(
                purchaseRequest,
                cancellationToken);

            await purchaseRequests.SaveChangesAsync(cancellationToken);

            return PurchaseRequestActionResult.Success(
                purchaseRequest.Id);
        }
        catch (ArgumentException exception)
        {
            return PurchaseRequestActionResult.Failure(
                exception.Message);
        }
    }
}
