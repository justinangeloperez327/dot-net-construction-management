using Domain.PurchaseOrders;

namespace Application.PurchaseOrders;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default);

    Task<bool> PurchaseOrderNumberExistsAsync(
        string purchaseOrderNumber,
        Guid? excludingPurchaseOrderId = null,
        CancellationToken cancellationToken = default);

    Task<decimal> GetCommittedQuantityForSourceItemAsync(
        Guid purchaseRequestItemId,
        Guid excludingPurchaseOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseOrder>> ListForProjectAsync(
        Guid projectId,
        PurchaseOrderStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseOrder>> ListByIdsAsync(
        IReadOnlyCollection<Guid> purchaseOrderIds,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
