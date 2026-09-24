using Application.Projects;
using Domain.Projects;

namespace Application.PurchaseRequests;

internal static class PurchaseRequestProjectGuard
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
            : "Purchase requests in a closed project cannot be changed.";
    }
}

public sealed record UpdatePurchaseRequestCommand(
    string RequestNumber,
    string Title,
    DateOnly? RequiredByDate,
    string? Purpose);

public sealed class UpdatePurchaseRequestHandler(
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        Guid purchaseRequestId,
        UpdatePurchaseRequestCommand command,
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

        var requestNumber = command.RequestNumber.Trim();

        if (await purchaseRequests.RequestNumberExistsAsync(
                purchaseRequest.ProjectId,
                requestNumber,
                purchaseRequest.Id,
                cancellationToken))
        {
            return PurchaseRequestActionResult.Failure(
                "A purchase request with this number already exists in the project.");
        }

        try
        {
            purchaseRequest.UpdateHeader(
                requestNumber,
                command.Title,
                command.RequiredByDate,
                command.Purpose);

            await purchaseRequests.SaveChangesAsync(cancellationToken);

            return PurchaseRequestActionResult.Success(
                purchaseRequest.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PurchaseRequestActionResult.Failure(
                exception.Message);
        }
    }
}

public sealed record PurchaseRequestItemCommand(
    string Description,
    decimal Quantity,
    string Unit,
    string? Remarks);

public sealed class AddPurchaseRequestItemHandler(
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        Guid purchaseRequestId,
        PurchaseRequestItemCommand command,
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
            purchaseRequest.AddItem(
                command.Description,
                command.Quantity,
                command.Unit,
                command.Remarks);

            await purchaseRequests.SaveChangesAsync(cancellationToken);

            return PurchaseRequestActionResult.Success(
                purchaseRequest.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PurchaseRequestActionResult.Failure(
                exception.Message);
        }
    }
}

public sealed class UpdatePurchaseRequestItemHandler(
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        Guid purchaseRequestId,
        Guid itemId,
        PurchaseRequestItemCommand command,
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
            purchaseRequest.UpdateItem(
                itemId,
                command.Description,
                command.Quantity,
                command.Unit,
                command.Remarks);

            await purchaseRequests.SaveChangesAsync(cancellationToken);

            return PurchaseRequestActionResult.Success(
                purchaseRequest.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return PurchaseRequestActionResult.Failure(
                exception.Message);
        }
    }
}

public sealed class RemovePurchaseRequestItemHandler(
    IPurchaseRequestRepository purchaseRequests,
    IProjectRepository projects)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        Guid purchaseRequestId,
        Guid itemId,
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
            purchaseRequest.RemoveItem(itemId);

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
