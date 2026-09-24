using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.Projects;
using Application.Users;
using Domain.Approvals;
using Domain.Projects;

namespace Application.PurchaseRequests;

public sealed record SubmitPurchaseRequestCommand(
    IReadOnlyList<ApprovalStepInput> Steps);

public sealed class SubmitPurchaseRequestHandler(
    IPurchaseRequestRepository purchaseRequests,
    IApprovalRequestRepository approvals,
    IProjectRepository projects,
    IUserDirectory users,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public async Task<PurchaseRequestActionResult> HandleAsync(
        Guid purchaseRequestId,
        SubmitPurchaseRequestCommand command,
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

        var project = await projects.GetByIdAsync(
            purchaseRequest.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return PurchaseRequestActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return PurchaseRequestActionResult.Failure(
                "Purchase requests in a closed project cannot be submitted.");
        }

        if (command.Steps.Count == 0)
        {
            return PurchaseRequestActionResult.Failure(
                "At least one approval step is required.");
        }

        if (await approvals.HasPendingForSubjectAsync(
                PurchaseRequestApproval.SubjectType,
                purchaseRequest.Id,
                cancellationToken))
        {
            return PurchaseRequestActionResult.Failure(
                "This purchase request already has a pending approval.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return PurchaseRequestActionResult.Failure(
                "An authenticated requester is required.");
        }

        if (purchaseRequest.RequestedByUserId != userId)
        {
            return PurchaseRequestActionResult.Failure(
                "Only the original requester can submit this purchase request.");
        }

        var approverIds = command.Steps
            .Select(step => step.ApproverUserId)
            .Distinct()
            .ToArray();

        var approvers = await users.ListByIdsAsync(
            approverIds,
            cancellationToken);

        if (approvers.Count != approverIds.Length ||
            approvers.Any(approver => !approver.IsActive))
        {
            return PurchaseRequestActionResult.Failure(
                "Every approval step must be assigned to an active user.");
        }

        try
        {
            var approval = ApprovalRequest.Create(
                purchaseRequest.ProjectId,
                PurchaseRequestApproval.SubjectType,
                purchaseRequest.Id,
                purchaseRequest.RequestNumber,
                $"Purchase request: {purchaseRequest.Title}",
                purchaseRequest.Purpose,
                userId,
                timeProvider.GetUtcNow(),
                command.Steps
                    .Select(step => new ApprovalStepAssignment(
                        step.Name,
                        step.ApproverUserId))
                    .ToArray());

            purchaseRequest.SubmitForApproval(approval.Id);

            await approvals.AddAsync(
                approval,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return PurchaseRequestActionResult.Success(
                purchaseRequest.Id,
                approval.Id);
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
