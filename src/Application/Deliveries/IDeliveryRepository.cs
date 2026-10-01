using Domain.Deliveries;

namespace Application.Deliveries;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(
        Guid deliveryId,
        CancellationToken cancellationToken = default);

    Task<bool> DeliveryNoteExistsAsync(
        Guid purchaseOrderId,
        string deliveryNoteNumber,
        Guid? excludingDeliveryId = null,
        CancellationToken cancellationToken = default);

    Task<decimal> GetReceivedQuantityForPurchaseOrderItemAsync(
        Guid purchaseOrderItemId,
        Guid? excludingDeliveryId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Delivery>> ListForProjectAsync(
        Guid projectId,
        DeliveryStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Delivery>> ListForPurchaseOrderAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        DeliveryStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Delivery delivery,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
