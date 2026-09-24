using Application.Common.Authentication;
using Application.Projects;
using Application.Users;
using Domain.Approvals;
using Domain.Projects;

namespace Application.Approvals;

public sealed record CreateApprovalRequestCommand(
    Guid? ProjectId,
    string SubjectType,
    Guid SubjectId,
    string Reference,
    string Title,
    string? Description,
    IReadOnlyList<ApprovalStepInput> Steps);

public sealed class CreateApprovalRequestHandler(
    IApprovalRequestRepository approvals,
    IProjectRepository projects,
    IUserDirectory users,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<ApprovalActionResult> HandleAsync(
        CreateApprovalRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProjectId is Guid projectId)
        {
            var project = await projects.GetByIdAsync(
                projectId,
                cancellationToken);

            if (project is null)
            {
                return ApprovalActionResult.Failure(
                    "Project was not found.");
            }

            if (project.Status == ProjectStatus.Closed)
            {
                return ApprovalActionResult.Failure(
                    "Approval requests cannot be created for a closed project.");
            }
        }

        if (await approvals.HasPendingForSubjectAsync(
                command.SubjectType.Trim(),
                command.SubjectId,
                cancellationToken))
        {
            return ApprovalActionResult.Failure(
                "A pending approval request already exists for this subject.");
        }

        if (command.Steps.Count == 0)
        {
            return ApprovalActionResult.Failure(
                "At least one approval step is required.");
        }

        var approverIds = command.Steps
            .Select(step => step.ApproverUserId)
            .Distinct()
            .ToArray();

        var approvers = await users.ListByIdsAsync(
            approverIds,
            cancellationToken);

        if (approvers.Count != approverIds.Length ||
            approvers.Any(user => !user.IsActive))
        {
            return ApprovalActionResult.Failure(
                "Every approval step must be assigned to an active user.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return ApprovalActionResult.Failure(
                "An authenticated requester is required.");
        }

        try
        {
            var request = ApprovalRequest.Create(
                command.ProjectId,
                command.SubjectType,
                command.SubjectId,
                command.Reference,
                command.Title,
                command.Description,
                userId,
                timeProvider.GetUtcNow(),
                command.Steps
                    .Select(step => new ApprovalStepAssignment(
                        step.Name,
                        step.ApproverUserId))
                    .ToArray());

            await approvals.AddAsync(
                request,
                cancellationToken);

            await approvals.SaveChangesAsync(cancellationToken);

            return ApprovalActionResult.Success(request.Id);
        }
        catch (ArgumentException exception)
        {
            return ApprovalActionResult.Failure(exception.Message);
        }
    }
}
