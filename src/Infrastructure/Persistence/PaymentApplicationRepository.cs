using Application.PaymentApplications;
using Domain.PaymentApplications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class PaymentApplicationRepository(
    ApplicationDbContext dbContext)
    : IPaymentApplicationRepository
{
    public Task<PaymentApplication?> GetByIdAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PaymentApplications
            .Include(application => application.Items)
            .SingleOrDefaultAsync(
                application => application.Id == paymentApplicationId,
                cancellationToken);
    }

    public Task<bool> ApplicationNumberExistsAsync(
        Guid purchaseOrderId,
        string applicationNumber,
        Guid? excludingPaymentApplicationId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PaymentApplications.AnyAsync(
            application =>
                application.PurchaseOrderId == purchaseOrderId &&
                application.ApplicationNumber == applicationNumber &&
                (!excludingPaymentApplicationId.HasValue ||
                 application.Id != excludingPaymentApplicationId.Value),
            cancellationToken);
    }

    public async Task<decimal> GetCommittedClaimQuantityForPurchaseOrderItemAsync(
        Guid purchaseOrderItemId,
        Guid excludingPaymentApplicationId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PaymentApplicationItems
            .AsNoTracking()
            .Where(item =>
                item.PurchaseOrderItemId == purchaseOrderItemId &&
                item.PaymentApplicationId != excludingPaymentApplicationId &&
                dbContext.PaymentApplications.Any(application =>
                    application.Id == item.PaymentApplicationId &&
                    (application.Status == PaymentApplicationStatus.PendingApproval ||
                     application.Status == PaymentApplicationStatus.Approved)))
            .SumAsync(
                item => (decimal?)item.ClaimedQuantity,
                cancellationToken)
            ?? 0m;
    }

    public async Task<IReadOnlyList<PaymentApplication>> ListForProjectAsync(
        Guid projectId,
        PaymentApplicationStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PaymentApplications
            .AsNoTracking()
            .Include(application => application.Items)
            .Where(application => application.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                application => application.Status == status.Value);
        }

        return await query
            .OrderByDescending(application => application.ApplicationDate)
            .ThenByDescending(application => application.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentApplication>> ListForPurchaseOrderAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PaymentApplications
            .AsNoTracking()
            .Include(application => application.Items)
            .Where(application =>
                application.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(application => application.ApplicationDate)
            .ThenByDescending(application => application.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        PaymentApplicationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PaymentApplications
            .AsNoTracking()
            .Where(application => application.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                application => application.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        PaymentApplication paymentApplication,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PaymentApplications.AddAsync(
            paymentApplication,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
