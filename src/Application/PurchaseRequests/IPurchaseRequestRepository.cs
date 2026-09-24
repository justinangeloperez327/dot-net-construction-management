using Domain.PurchaseRequests;

namespace Application.PurchaseRequests;

public interface IPurchaseRequestRepository
{
    Task<PurchaseRequest?> GetByIdAsync(
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default);

    Task<bool> RequestNumberExistsAsync(
        Guid projectId,
        string requestNumber,
        Guid? excludingPurchaseRequestId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseRequest>> ListForProjectAsync(
        Guid projectId,
        PurchaseRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        PurchaseRequestStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PurchaseRequest purchaseRequest,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
