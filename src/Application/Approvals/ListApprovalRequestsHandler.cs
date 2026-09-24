using Application.Common.Authentication;
using Application.Users;
using Domain.Approvals;

namespace Application.Approvals;

public sealed record ListApprovalRequestsQuery(
    ApprovalRequestStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListApprovalRequestsHandler(
    IApprovalRequestRepository approvals,
    IUserDirectory users,
    ICurrentUser currentUser)
{
    public async Task<ApprovalRequestListResult> HandleAsync(
        ListApprovalRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var current = await currentUser.GetAsync(cancellationToken);

        if (!current.IsAuthenticated ||
            current.UserId is not Guid userId)
        {
            return new ApprovalRequestListResult(
                [],
                1,
                Math.Clamp(query.PageSize, 1, 100),
                0);
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await approvals.CountForUserAsync(
            userId,
            query.Status,
            cancellationToken);

        var requests = await approvals.ListForUserAsync(
            userId,
            query.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var requesterIds = requests
            .Select(request => request.RequestedByUserId)
            .Distinct()
            .ToArray();

        var directory = await users.ListByIdsAsync(
            requesterIds,
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        return new ApprovalRequestListResult(
            requests.Select(request => new ApprovalRequestSummary(
                    request.Id,
                    request.ProjectId,
                    request.SubjectType,
                    request.SubjectId,
                    request.Reference,
                    request.Title,
                    request.Status,
                    usersById.GetValueOrDefault(
                            request.RequestedByUserId)?.Email
                        ?? "Unknown user",
                    request.RequestedAt,
                    request.CurrentStep?.Name,
                    request.CurrentStep?.ApproverUserId == userId))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
