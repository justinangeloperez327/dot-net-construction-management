using Application.PurchaseRequests;
using Application.Suppliers;
using Application.Users;
using Domain.PurchaseOrders;

namespace Application.PurchaseOrders;

public sealed class GetPurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrders,
    IPurchaseRequestRepository purchaseRequests,
    ISupplierRepository suppliers,
    IUserDirectory users)
{
    public async Task<PurchaseOrderDetails?> HandleAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            purchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return null;
        }

        var purchaseRequest = await purchaseRequests.GetByIdAsync(
            purchaseOrder.PurchaseRequestId,
            cancellationToken);

        var supplier = await suppliers.GetByIdAsync(
            purchaseOrder.SupplierId,
            cancellationToken);

        var creator = await users.GetAsync(
            purchaseOrder.CreatedByUserId,
            cancellationToken);

        return new PurchaseOrderDetails(
            purchaseOrder.Id,
            purchaseOrder.ProjectId,
            purchaseOrder.PurchaseRequestId,
            purchaseRequest?.RequestNumber ?? "Unknown PR",
            purchaseOrder.SupplierId,
            supplier?.Name ?? "Unknown supplier",
            purchaseOrder.PurchaseOrderNumber,
            purchaseOrder.OrderDate,
            purchaseOrder.ExpectedDeliveryDate,
            purchaseOrder.Currency,
            purchaseOrder.DeliveryAddress,
            purchaseOrder.DeliveryTerms,
            purchaseOrder.PaymentTerms,
            purchaseOrder.Notes,
            purchaseOrder.Status,
            creator?.Email ?? "Unknown user",
            purchaseOrder.CreatedAt,
            purchaseOrder.ApprovalRequestId,
            purchaseOrder.Subtotal,
            purchaseOrder.DiscountTotal,
            purchaseOrder.TaxTotal,
            purchaseOrder.GrandTotal,
            purchaseOrder.Items
                .OrderBy(item => item.Description)
                .Select(item => new PurchaseOrderItemDetails(
                    item.Id,
                    item.PurchaseRequestItemId,
                    item.Description,
                    item.Quantity,
                    item.Unit,
                    item.UnitPrice,
                    item.DiscountPercent,
                    item.TaxPercent,
                    item.Remarks,
                    item.GrossAmount,
                    item.DiscountAmount,
                    item.TaxAmount,
                    item.TotalAmount))
                .ToArray());
    }
}

public sealed record ListPurchaseOrdersQuery(
    Guid ProjectId,
    PurchaseOrderStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListPurchaseOrdersHandler(
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers)
{
    public async Task<PurchaseOrderListResult> HandleAsync(
        ListPurchaseOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await purchaseOrders.CountForProjectAsync(
            query.ProjectId,
            query.Status,
            cancellationToken);

        var orders = await purchaseOrders.ListForProjectAsync(
            query.ProjectId,
            query.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var supplierIds = orders
            .Select(order => order.SupplierId)
            .Distinct()
            .ToArray();

        var supplierDirectory = await suppliers.ListByIdsAsync(
            supplierIds,
            cancellationToken);

        var suppliersById = supplierDirectory.ToDictionary(
            supplier => supplier.Id);

        return new PurchaseOrderListResult(
            orders.Select(order => new PurchaseOrderSummary(
                    order.Id,
                    order.PurchaseOrderNumber,
                    suppliersById.GetValueOrDefault(
                            order.SupplierId)?.Name
                        ?? "Unknown supplier",
                    order.Currency,
                    order.GrandTotal,
                    order.OrderDate,
                    order.ExpectedDeliveryDate,
                    order.Status))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
