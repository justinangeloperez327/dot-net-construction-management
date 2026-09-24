using Application.Common.Authentication;
using Application.Users;

namespace Application.Approvals;

public sealed class GetApprovalRequestHandler(
    IApprovalRequestRepository approvals,
    IUserDirectory users,
    ICurrentUser currentUser)
{
    public async Task<ApprovalRequestDetails?> HandleAsync(
        Guid approvalRequestId,
        CancellationToken cancellationToken = default)
    {
        var request = await approvals.GetByIdAsync(
            approvalRequestId,
            cancellationToken);

        if (request is null)
        {
            return null;
        }

        var current = await currentUser.GetAsync(cancellationToken);

        if (!current.IsAuthenticated ||
            current.UserId is not Guid currentUserId)
        {
            return null;
        }

        var isParticipant =
            request.RequestedByUserId == currentUserId ||
            request.Steps.Any(
                step => step.ApproverUserId == currentUserId);

        if (!isParticipant)
        {
            return null;
        }

        var userIds = request.Steps
            .Select(step => step.ApproverUserId)
            .Append(request.RequestedByUserId)
            .Distinct()
            .ToArray();

        var directory = await users.ListByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        string UserName(Guid userId) =>
            usersById.GetValueOrDefault(userId)?.Email
            ?? "Unknown user";

        var currentStep = request.CurrentStep;

        return new ApprovalRequestDetails(
            request.Id,
            request.ProjectId,
            request.SubjectType,
            request.SubjectId,
            request.Reference,
            request.Title,
            request.Description,
            request.Status,
            UserName(request.RequestedByUserId),
            request.RequestedAt,
            request.CompletedAt,
            request.CancelledAt,
            request.CancellationReason,
            request.Steps
                .OrderBy(step => step.StepNumber)
                .Select(step => new ApprovalStepDetails(
                    step.Id,
                    step.StepNumber,
                    step.Name,
                    step.ApproverUserId,
                    UserName(step.ApproverUserId),
                    step.Status,
                    step.DecidedAt,
                    step.Comments))
                .ToArray(),
            currentStep?.ApproverUserId == currentUserId,
            request.Status == Domain.Approvals.ApprovalRequestStatus.Pending &&
            request.RequestedByUserId == currentUserId);
    }
}
