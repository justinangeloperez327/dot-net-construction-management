using Application.Common.Authentication;
using Application.Common.Persistence;
using Domain.Approvals;

namespace Application.Approvals;

public sealed record DecideApprovalStepCommand(
    ApprovalStepDecision Decision,
    string? Comments);

public sealed class DecideApprovalStepHandler(
    IApprovalRequestRepository approvals,
    IEnumerable<IApprovalSubjectOutcomeHandler> outcomeHandlers,
    IUnitOfWork unitOfWork,
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

            await ApplyOutcomeAsync(
                request,
                outcomeHandlers,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApprovalActionResult.Success(request.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return ApprovalActionResult.Failure(exception.Message);
        }
    }

    private static async Task ApplyOutcomeAsync(
        ApprovalRequest request,
        IEnumerable<IApprovalSubjectOutcomeHandler> outcomeHandlers,
        CancellationToken cancellationToken)
    {
        var handler = outcomeHandlers.SingleOrDefault(
            item => string.Equals(
                item.SubjectType,
                request.SubjectType,
                StringComparison.Ordinal));

        if (handler is not null)
        {
            await handler.ApplyAsync(
                request,
                cancellationToken);
        }
    }
}

public sealed class CancelApprovalRequestHandler(
    IApprovalRequestRepository approvals,
    IEnumerable<IApprovalSubjectOutcomeHandler> outcomeHandlers,
    IUnitOfWork unitOfWork,
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

            var handler = outcomeHandlers.SingleOrDefault(
                item => string.Equals(
                    item.SubjectType,
                    request.SubjectType,
                    StringComparison.Ordinal));

            if (handler is not null)
            {
                await handler.ApplyAsync(
                    request,
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ApprovalActionResult.Success(request.Id);
        }
        catch (InvalidOperationException exception)
        {
            return ApprovalActionResult.Failure(exception.Message);
        }
    }
}
