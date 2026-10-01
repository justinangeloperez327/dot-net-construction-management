using Application.Commercial;
using Domain.Commercial;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class CommercialCertificationRepository(
    ApplicationDbContext dbContext)
    : ICommercialCertificationRepository
{
    public Task<CommercialCertification?> GetByIdAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CommercialCertifications
            .Include(certification => certification.OtherDeductions)
            .SingleOrDefaultAsync(
                certification => certification.Id == certificationId,
                cancellationToken);
    }

    public Task<CommercialCertification?> GetByPaymentApplicationIdAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CommercialCertifications
            .Include(certification => certification.OtherDeductions)
            .SingleOrDefaultAsync(
                certification =>
                    certification.PaymentApplicationId == paymentApplicationId,
                cancellationToken);
    }

    public Task<bool> CertificateNumberExistsAsync(
        Guid projectId,
        string certificateNumber,
        Guid? excludingCertificationId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CommercialCertifications.AnyAsync(
            certification =>
                certification.ProjectId == projectId &&
                certification.CertificateNumber == certificateNumber &&
                (!excludingCertificationId.HasValue ||
                 certification.Id != excludingCertificationId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<CommercialCertification>> ListForProjectAsync(
        Guid projectId,
        CommercialCertificationStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.CommercialCertifications
            .AsNoTracking()
            .Include(certification => certification.OtherDeductions)
            .Where(certification => certification.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                certification => certification.Status == status.Value);
        }

        return await query
            .OrderByDescending(certification => certification.CertificateDate)
            .ThenByDescending(certification => certification.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        CommercialCertificationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.CommercialCertifications
            .AsNoTracking()
            .Where(certification => certification.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                certification => certification.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        CommercialCertification certification,
        CancellationToken cancellationToken = default)
    {
        await dbContext.CommercialCertifications.AddAsync(
            certification,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
