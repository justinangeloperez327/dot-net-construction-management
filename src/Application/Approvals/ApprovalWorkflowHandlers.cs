using Application.Common.Authentication;
using Domain.Approvals;

namespace Application.Approvals;

public sealed record DecideApprovalStepCommand(
    ApprovalStepDecision Decision,
    string? Comments);

public sealed class DecideApprovalStepHandler(
    IApprovalRequestRepository approvals,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<ApprovalActionResult> HandleAsync(
        Guid approvalRequestId,
        Guid stepId,
        DecideApprovalStepCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await approvals.GetByIdAsync(
            approvalRequestId,
            cancellationToken);

        if (request is null)
        {
            return ApprovalActionResult.Failure(
                "Approval request was not found.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return ApprovalActionResult.Failure(
                "An authenticated approver is required.");
        }

        try
        {
            request.Decide(
                stepId,
                userId,
                command.Decision,
                timeProvider.GetUtcNow(),
                command.Comments);

            await approvals.SaveChangesAsync(cancellationToken);

            return ApprovalActionResult.Success(request.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return ApprovalActionResult.Failure(exception.Message);
        }
    }
}

public sealed class CancelApprovalRequestHandler(
    IApprovalRequestRepository approvals,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<ApprovalActionResult> HandleAsync(
        Guid approvalRequestId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var request = await approvals.GetByIdAsync(
            approvalRequestId,
            cancellationToken);

        if (request is null)
        {
            return ApprovalActionResult.Failure(
                "Approval request was not found.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return ApprovalActionResult.Failure(
                "An authenticated requester is required.");
        }

        try
        {
            request.Cancel(
                userId,
                timeProvider.GetUtcNow(),
                reason);

            await approvals.SaveChangesAsync(cancellationToken);

            return ApprovalActionResult.Success(request.Id);
        }
        catch (InvalidOperationException exception)
        {
            return ApprovalActionResult.Failure(exception.Message);
        }
    }
}
