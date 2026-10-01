using Domain.Commercial;

namespace Application.Commercial;

public interface ICommercialCertificationRepository
{
    Task<CommercialCertification?> GetByIdAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default);

    Task<CommercialCertification?> GetByPaymentApplicationIdAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default);

    Task<bool> CertificateNumberExistsAsync(
        Guid projectId,
        string certificateNumber,
        Guid? excludingCertificationId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommercialCertification>> ListForProjectAsync(
        Guid projectId,
        CommercialCertificationStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        CommercialCertificationStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CommercialCertification certification,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
