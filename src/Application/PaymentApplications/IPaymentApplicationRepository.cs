using Domain.PaymentApplications;

namespace Application.PaymentApplications;

public interface IPaymentApplicationRepository
{
    Task<PaymentApplication?> GetByIdAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default);

    Task<bool> ApplicationNumberExistsAsync(
        Guid purchaseOrderId,
        string applicationNumber,
        Guid? excludingPaymentApplicationId = null,
        CancellationToken cancellationToken = default);

    Task<decimal> GetCommittedClaimQuantityForPurchaseOrderItemAsync(
        Guid purchaseOrderItemId,
        Guid excludingPaymentApplicationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentApplication>> ListForProjectAsync(
        Guid projectId,
        PaymentApplicationStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentApplication>> ListForPurchaseOrderAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentApplication>> ListByIdsAsync(
        IReadOnlyCollection<Guid> paymentApplicationIds,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        PaymentApplicationStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PaymentApplication paymentApplication,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
