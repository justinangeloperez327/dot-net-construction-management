using Application.PurchaseRequests;
using Domain.PurchaseRequests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class PurchaseRequestRepository(
    ApplicationDbContext dbContext)
    : IPurchaseRequestRepository
{
    public Task<PurchaseRequest?> GetByIdAsync(
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PurchaseRequests
            .Include(request => request.Items)
            .SingleOrDefaultAsync(
                request => request.Id == purchaseRequestId,
                cancellationToken);
    }

    public Task<bool> RequestNumberExistsAsync(
        Guid projectId,
        string requestNumber,
        Guid? excludingPurchaseRequestId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PurchaseRequests.AnyAsync(
            request =>
                request.ProjectId == projectId &&
                request.RequestNumber == requestNumber &&
                (!excludingPurchaseRequestId.HasValue ||
                 request.Id != excludingPurchaseRequestId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseRequest>> ListForProjectAsync(
        Guid projectId,
        PurchaseRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseRequests
            .AsNoTracking()
            .Include(request => request.Items)
            .Where(request => request.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                request => request.Status == status.Value);
        }

        return await query
            .OrderByDescending(request => request.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        PurchaseRequestStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseRequests
            .AsNoTracking()
            .Where(request => request.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                request => request.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        PurchaseRequest purchaseRequest,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PurchaseRequests.AddAsync(
            purchaseRequest,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
