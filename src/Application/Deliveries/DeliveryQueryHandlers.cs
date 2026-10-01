using Application.PurchaseOrders;
using Application.Suppliers;
using Application.Users;
using Domain.Deliveries;

namespace Application.Deliveries;

public sealed record AvailableDeliveryItem(
    Guid PurchaseOrderItemId,
    string Description,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    string Unit);

public sealed class ListAvailableDeliveryItemsHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders)
{
    public async Task<IReadOnlyList<AvailableDeliveryItem>> HandleAsync(
        Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await deliveries.GetByIdAsync(
            deliveryId,
            cancellationToken);

        if (delivery is null)
        {
            return [];
        }

        var order = await purchaseOrders.GetByIdAsync(
            delivery.PurchaseOrderId,
            cancellationToken);

        if (order is null)
        {
            return [];
        }

        var existingSourceIds = delivery.Items
            .Select(item => item.PurchaseOrderItemId)
            .ToHashSet();

        var result = new List<AvailableDeliveryItem>();

        foreach (var orderItem in order.Items)
        {
            if (existingSourceIds.Contains(orderItem.Id))
            {
                continue;
            }

            var received = await deliveries
                .GetReceivedQuantityForPurchaseOrderItemAsync(
                    orderItem.Id,
                    delivery.Id,
                    cancellationToken);

            var remaining = orderItem.Quantity - received;

            if (remaining <= 0)
            {
                continue;
            }

            result.Add(new AvailableDeliveryItem(
                orderItem.Id,
                orderItem.Description,
                orderItem.Quantity,
                received,
                remaining,
                orderItem.Unit));
        }

        return result;
    }
}

public sealed class GetDeliveryHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    IUserDirectory users)
{
    public async Task<DeliveryDetails?> HandleAsync(
        Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await deliveries.GetByIdAsync(
            deliveryId,
            cancellationToken);

        if (delivery is null)
        {
            return null;
        }

        var order = await purchaseOrders.GetByIdAsync(
            delivery.PurchaseOrderId,
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        var supplier = await suppliers.GetByIdAsync(
            order.SupplierId,
            cancellationToken);

        var userIds = new[]
        {
            delivery.CreatedByUserId,
            delivery.ReceivedByUserId
        }
        .Where(id => id.HasValue)
        .Select(id => id!.Value)
        .Distinct()
        .ToArray();

        var directory = await users.ListByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        var itemDetails = new List<DeliveryItemDetails>();

        foreach (var item in delivery.Items)
        {
            var orderItem = order.Items.Single(
                source => source.Id == item.PurchaseOrderItemId);

            var previouslyReceived = await deliveries
                .GetReceivedQuantityForPurchaseOrderItemAsync(
                    item.PurchaseOrderItemId,
                    delivery.Id,
                    cancellationToken);

            itemDetails.Add(new DeliveryItemDetails(
                item.Id,
                item.PurchaseOrderItemId,
                item.Description,
                orderItem.Quantity,
                previouslyReceived,
                item.Quantity,
                orderItem.Quantity - previouslyReceived - item.Quantity,
                item.Unit,
                item.Remarks));
        }

        return new DeliveryDetails(
            delivery.Id,
            delivery.ProjectId,
            delivery.PurchaseOrderId,
            order.PurchaseOrderNumber,
            supplier?.Name ?? "Unknown supplier",
            delivery.DeliveryNoteNumber,
            delivery.DeliveryDate,
            delivery.VehicleReference,
            delivery.Remarks,
            delivery.Status,
            usersById.GetValueOrDefault(
                    delivery.CreatedByUserId)?.Email
                ?? "Unknown user",
            delivery.CreatedAt,
            delivery.ReceivedByUserId is Guid receivedByUserId
                ? usersById.GetValueOrDefault(receivedByUserId)?.Email
                : null,
            delivery.ReceivedAt,
            itemDetails);
    }
}

public sealed record ListDeliveriesQuery(
    Guid ProjectId,
    DeliveryStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListDeliveriesHandler(
    IDeliveryRepository deliveries,
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    IUserDirectory users)
{
    public async Task<DeliveryListResult> HandleAsync(
        ListDeliveriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await deliveries.CountForProjectAsync(
            query.ProjectId,
            query.Status,
            cancellationToken);

        var deliveryList = await deliveries.ListForProjectAsync(
            query.ProjectId,
            query.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var orderIds = deliveryList
            .Select(delivery => delivery.PurchaseOrderId)
            .Distinct()
            .ToArray();

        var orders = await purchaseOrders.ListByIdsAsync(
            orderIds,
            cancellationToken);

        var ordersById = orders.ToDictionary(order => order.Id);

        var supplierIds = orders
            .Select(order => order.SupplierId)
            .Distinct()
            .ToArray();

        var supplierDirectory = await suppliers.ListByIdsAsync(
            supplierIds,
            cancellationToken);

        var suppliersById = supplierDirectory.ToDictionary(
            supplier => supplier.Id);

        var userIds = deliveryList
            .SelectMany(delivery => new Guid?[]
            {
                delivery.CreatedByUserId,
                delivery.ReceivedByUserId
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var userDirectory = await users.ListByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = userDirectory.ToDictionary(user => user.Id);

        return new DeliveryListResult(
            deliveryList.Select(delivery =>
            {
                var order = ordersById.GetValueOrDefault(
                    delivery.PurchaseOrderId);

                var supplierName = order is null
                    ? "Unknown supplier"
                    : suppliersById.GetValueOrDefault(
                            order.SupplierId)?.Name
                        ?? "Unknown supplier";

                return new DeliverySummary(
                    delivery.Id,
                    delivery.DeliveryNoteNumber,
                    order?.PurchaseOrderNumber ?? "Unknown PO",
                    supplierName,
                    delivery.DeliveryDate,
                    delivery.Status,
                    usersById.GetValueOrDefault(
                            delivery.CreatedByUserId)?.Email
                        ?? "Unknown user",
                    delivery.ReceivedByUserId is Guid receiverId
                        ? usersById.GetValueOrDefault(receiverId)?.Email
                        : null,
                    delivery.ReceivedAt,
                    delivery.Items.Count);
            }).ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
