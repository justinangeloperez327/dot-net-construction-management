using Application.Deliveries;
using Domain.Deliveries;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DeliveryRepository(
    ApplicationDbContext dbContext)
    : IDeliveryRepository
{
    public Task<Delivery?> GetByIdAsync(
        Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Deliveries
            .Include(delivery => delivery.Items)
            .SingleOrDefaultAsync(
                delivery => delivery.Id == deliveryId,
                cancellationToken);
    }

    public Task<bool> DeliveryNoteExistsAsync(
        Guid purchaseOrderId,
        string deliveryNoteNumber,
        Guid? excludingDeliveryId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Deliveries.AnyAsync(
            delivery =>
                delivery.PurchaseOrderId == purchaseOrderId &&
                delivery.DeliveryNoteNumber == deliveryNoteNumber &&
                (!excludingDeliveryId.HasValue ||
                 delivery.Id != excludingDeliveryId.Value),
            cancellationToken);
    }

    public async Task<decimal> GetReceivedQuantityForPurchaseOrderItemAsync(
        Guid purchaseOrderItemId,
        Guid? excludingDeliveryId = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DeliveryItems
            .AsNoTracking()
            .Where(item =>
                item.PurchaseOrderItemId == purchaseOrderItemId &&
                (!excludingDeliveryId.HasValue ||
                 item.DeliveryId != excludingDeliveryId.Value) &&
                dbContext.Deliveries.Any(delivery =>
                    delivery.Id == item.DeliveryId &&
                    delivery.Status == DeliveryStatus.Received))
            .SumAsync(
                item => (decimal?)item.Quantity,
                cancellationToken)
            ?? 0m;
    }

    public async Task<IReadOnlyList<Delivery>> ListForProjectAsync(
        Guid projectId,
        DeliveryStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Deliveries
            .AsNoTracking()
            .Include(delivery => delivery.Items)
            .Where(delivery => delivery.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                delivery => delivery.Status == status.Value);
        }

        return await query
            .OrderByDescending(delivery => delivery.DeliveryDate)
            .ThenByDescending(delivery => delivery.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Delivery>> ListForPurchaseOrderAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Deliveries
            .AsNoTracking()
            .Include(delivery => delivery.Items)
            .Where(delivery =>
                delivery.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(delivery => delivery.DeliveryDate)
            .ThenByDescending(delivery => delivery.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        DeliveryStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Deliveries
            .AsNoTracking()
            .Where(delivery => delivery.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                delivery => delivery.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Delivery delivery,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Deliveries.AddAsync(
            delivery,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
