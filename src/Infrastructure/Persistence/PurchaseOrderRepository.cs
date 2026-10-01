using Application.PurchaseOrders;
using Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class PurchaseOrderRepository(
    ApplicationDbContext dbContext)
    : IPurchaseOrderRepository
{
    public Task<PurchaseOrder?> GetByIdAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PurchaseOrders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.Id == purchaseOrderId,
                cancellationToken);
    }

    public Task<bool> PurchaseOrderNumberExistsAsync(
        string purchaseOrderNumber,
        Guid? excludingPurchaseOrderId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PurchaseOrders.AnyAsync(
            order =>
                order.PurchaseOrderNumber == purchaseOrderNumber &&
                (!excludingPurchaseOrderId.HasValue ||
                 order.Id != excludingPurchaseOrderId.Value),
            cancellationToken);
    }

    public async Task<decimal> GetCommittedQuantityForSourceItemAsync(
        Guid purchaseRequestItemId,
        Guid excludingPurchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PurchaseOrderItems
            .AsNoTracking()
            .Where(item =>
                item.PurchaseRequestItemId == purchaseRequestItemId &&
                item.PurchaseOrderId != excludingPurchaseOrderId &&
                dbContext.PurchaseOrders.Any(order =>
                    order.Id == item.PurchaseOrderId &&
                    (order.Status == PurchaseOrderStatus.PendingApproval ||
                     order.Status == PurchaseOrderStatus.Approved)))
            .SumAsync(
                item => (decimal?)item.Quantity,
                cancellationToken)
            ?? 0m;
    }

    public async Task<IReadOnlyList<PurchaseOrder>> ListForProjectAsync(
        Guid projectId,
        PurchaseOrderStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                order => order.Status == status.Value);
        }

        return await query
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> ListByIdsAsync(
        IReadOnlyCollection<Guid> purchaseOrderIds,
        CancellationToken cancellationToken = default)
    {
        if (purchaseOrderIds.Count == 0)
        {
            return [];
        }

        return await dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => purchaseOrderIds.Contains(order.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PurchaseOrders
            .AsNoTracking()
            .Where(order => order.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                order => order.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PurchaseOrders.AddAsync(
            purchaseOrder,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
