using Application.Deliveries;
using Application.PurchaseOrders;
using Application.Suppliers;
using Application.Users;
using Domain.PaymentApplications;

namespace Application.PaymentApplications;

public sealed class ListAvailablePaymentApplicationItemsHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    IDeliveryRepository deliveries)
{
    public async Task<IReadOnlyList<AvailablePaymentApplicationItem>> HandleAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default)
    {
        var paymentApplication = await paymentApplications.GetByIdAsync(
            paymentApplicationId,
            cancellationToken);

        if (paymentApplication is null)
        {
            return [];
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            paymentApplication.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return [];
        }

        var existingItemIds = paymentApplication.Items
            .Select(item => item.PurchaseOrderItemId)
            .ToHashSet();

        var result = new List<AvailablePaymentApplicationItem>();

        foreach (var orderItem in purchaseOrder.Items)
        {
            if (existingItemIds.Contains(orderItem.Id))
            {
                continue;
            }

            var receivedQuantity =
                await deliveries.GetReceivedQuantityForPurchaseOrderItemAsync(
                    orderItem.Id,
                    cancellationToken: cancellationToken);

            var committedClaimQuantity =
                await paymentApplications
                    .GetCommittedClaimQuantityForPurchaseOrderItemAsync(
                        orderItem.Id,
                        paymentApplication.Id,
                        cancellationToken);

            var availableQuantity =
                receivedQuantity - committedClaimQuantity;

            if (availableQuantity <= 0)
            {
                continue;
            }

            result.Add(new AvailablePaymentApplicationItem(
                orderItem.Id,
                orderItem.Description,
                orderItem.Quantity,
                receivedQuantity,
                committedClaimQuantity,
                availableQuantity,
                orderItem.Unit,
                orderItem.UnitPrice,
                orderItem.DiscountPercent,
                orderItem.TaxPercent));
        }

        return result;
    }
}

public sealed class GetPaymentApplicationHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    IDeliveryRepository deliveries,
    ISupplierRepository suppliers,
    IUserDirectory users)
{
    public async Task<PaymentApplicationDetails?> HandleAsync(
        Guid paymentApplicationId,
        CancellationToken cancellationToken = default)
    {
        var paymentApplication = await paymentApplications.GetByIdAsync(
            paymentApplicationId,
            cancellationToken);

        if (paymentApplication is null)
        {
            return null;
        }

        var purchaseOrder = await purchaseOrders.GetByIdAsync(
            paymentApplication.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return null;
        }

        var supplier = await suppliers.GetByIdAsync(
            purchaseOrder.SupplierId,
            cancellationToken);

        var creator = await users.GetAsync(
            paymentApplication.CreatedByUserId,
            cancellationToken);

        var itemDetails = new List<PaymentApplicationItemDetails>();

        foreach (var item in paymentApplication.Items)
        {
            var receivedQuantity =
                await deliveries.GetReceivedQuantityForPurchaseOrderItemAsync(
                    item.PurchaseOrderItemId,
                    cancellationToken: cancellationToken);

            var committedClaimQuantity =
                await paymentApplications
                    .GetCommittedClaimQuantityForPurchaseOrderItemAsync(
                        item.PurchaseOrderItemId,
                        paymentApplication.Id,
                        cancellationToken);

            itemDetails.Add(new PaymentApplicationItemDetails(
                item.Id,
                item.PurchaseOrderItemId,
                item.Description,
                receivedQuantity,
                committedClaimQuantity,
                item.ClaimedQuantity,
                receivedQuantity
                    - committedClaimQuantity
                    - item.ClaimedQuantity,
                item.Unit,
                item.UnitPrice,
                item.DiscountPercent,
                item.TaxPercent,
                item.Remarks,
                item.GrossAmount,
                item.DiscountAmount,
                item.TaxAmount,
                item.TotalAmount));
        }

        return new PaymentApplicationDetails(
            paymentApplication.Id,
            paymentApplication.ProjectId,
            paymentApplication.PurchaseOrderId,
            purchaseOrder.PurchaseOrderNumber,
            purchaseOrder.SupplierId,
            supplier?.Name ?? "Unknown supplier",
            purchaseOrder.Currency,
            paymentApplication.ApplicationNumber,
            paymentApplication.ApplicationDate,
            paymentApplication.PeriodFrom,
            paymentApplication.PeriodTo,
            paymentApplication.Notes,
            paymentApplication.Status,
            creator?.Email ?? "Unknown user",
            paymentApplication.CreatedAt,
            paymentApplication.ApprovalRequestId,
            paymentApplication.GrossAmount,
            paymentApplication.DiscountAmount,
            paymentApplication.TaxAmount,
            paymentApplication.ClaimedAmount,
            itemDetails);
    }
}

public sealed record ListPaymentApplicationsQuery(
    Guid ProjectId,
    PaymentApplicationStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListPaymentApplicationsHandler(
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    IUserDirectory users)
{
    public async Task<PaymentApplicationListResult> HandleAsync(
        ListPaymentApplicationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await paymentApplications.CountForProjectAsync(
            query.ProjectId,
            query.Status,
            cancellationToken);

        var applications = await paymentApplications.ListForProjectAsync(
            query.ProjectId,
            query.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var orderIds = applications
            .Select(application => application.PurchaseOrderId)
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

        var creatorIds = applications
            .Select(application => application.CreatedByUserId)
            .Distinct()
            .ToArray();

        var userDirectory = await users.ListByIdsAsync(
            creatorIds,
            cancellationToken);

        var usersById = userDirectory.ToDictionary(user => user.Id);

        return new PaymentApplicationListResult(
            applications.Select(application =>
            {
                var order = ordersById.GetValueOrDefault(
                    application.PurchaseOrderId);

                var supplierName = order is null
                    ? "Unknown supplier"
                    : suppliersById.GetValueOrDefault(
                            order.SupplierId)?.Name
                        ?? "Unknown supplier";

                return new PaymentApplicationSummary(
                    application.Id,
                    application.ApplicationNumber,
                    order?.PurchaseOrderNumber ?? "Unknown PO",
                    supplierName,
                    application.ApplicationDate,
                    order?.Currency ?? "---",
                    application.ClaimedAmount,
                    application.Status,
                    usersById.GetValueOrDefault(
                            application.CreatedByUserId)?.Email
                        ?? "Unknown user");
            }).ToArray(),
            page,
            pageSize,
            totalCount);
    }
}

public sealed class ListPaymentApplicationApproversHandler(
    IUserDirectory users)
{
    public Task<IReadOnlyList<UserDirectoryEntry>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return users.ListActiveAsync(cancellationToken);
    }
}
