using Application.Users;

namespace Application.PurchaseOrders;

public sealed class ListPurchaseOrderApproversHandler(
    IUserDirectory users)
{
    public Task<IReadOnlyList<UserDirectoryEntry>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return users.ListActiveAsync(cancellationToken);
    }
}
