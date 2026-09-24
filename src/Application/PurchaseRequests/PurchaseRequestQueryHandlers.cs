using Application.Users;
using Domain.PurchaseRequests;

namespace Application.PurchaseRequests;

public sealed class GetPurchaseRequestHandler(
    IPurchaseRequestRepository purchaseRequests,
    IUserDirectory users)
{
    public async Task<PurchaseRequestDetails?> HandleAsync(
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await purchaseRequests.GetByIdAsync(
            purchaseRequestId,
            cancellationToken);

        if (purchaseRequest is null)
        {
            return null;
        }

        var requester = await users.GetAsync(
            purchaseRequest.RequestedByUserId,
            cancellationToken);

        return new PurchaseRequestDetails(
            purchaseRequest.Id,
            purchaseRequest.ProjectId,
            purchaseRequest.RequestNumber,
            purchaseRequest.Title,
            purchaseRequest.RequiredByDate,
            purchaseRequest.Purpose,
            purchaseRequest.Status,
            requester?.Email ?? "Unknown user",
            purchaseRequest.CreatedAt,
            purchaseRequest.ApprovalRequestId,
            purchaseRequest.Items
                .OrderBy(item => item.Description)
                .Select(item => new PurchaseRequestItemDetails(
                    item.Id,
                    item.Description,
                    item.Quantity,
                    item.Unit,
                    item.Remarks))
                .ToArray());
    }
}

public sealed record ListPurchaseRequestsQuery(
    Guid ProjectId,
    PurchaseRequestStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListPurchaseRequestsHandler(
    IPurchaseRequestRepository purchaseRequests,
    IUserDirectory users)
{
    public async Task<PurchaseRequestListResult> HandleAsync(
        ListPurchaseRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await purchaseRequests.CountForProjectAsync(
            query.ProjectId,
            query.Status,
            cancellationToken);

        var items = await purchaseRequests.ListForProjectAsync(
            query.ProjectId,
            query.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var userIds = items
            .Select(item => item.RequestedByUserId)
            .Distinct()
            .ToArray();

        var directory = await users.ListByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        return new PurchaseRequestListResult(
            items.Select(item => new PurchaseRequestSummary(
                    item.Id,
                    item.RequestNumber,
                    item.Title,
                    item.RequiredByDate,
                    item.Status,
                    usersById.GetValueOrDefault(
                            item.RequestedByUserId)?.Email
                        ?? "Unknown user",
                    item.CreatedAt,
                    item.Items.Count))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}

public sealed class ListPurchaseRequestApproversHandler(
    IUserDirectory users)
{
    public Task<IReadOnlyList<UserDirectoryEntry>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return users.ListActiveAsync(cancellationToken);
    }
}
