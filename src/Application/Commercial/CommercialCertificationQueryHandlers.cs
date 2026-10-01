using Application.PaymentApplications;
using Application.PurchaseOrders;
using Application.Suppliers;
using Application.Users;
using Domain.Commercial;

namespace Application.Commercial;

public sealed class GetCommercialCertificationHandler(
    ICommercialCertificationRepository certifications,
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers,
    IUserDirectory users)
{
    public async Task<CommercialCertificationDetails?> HandleAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        var certification = await certifications.GetByIdAsync(
            certificationId,
            cancellationToken);

        if (certification is null)
        {
            return null;
        }

        var paymentApplication = await paymentApplications.GetByIdAsync(
            certification.PaymentApplicationId,
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
            certification.CreatedByUserId,
            cancellationToken);

        return new CommercialCertificationDetails(
            certification.Id,
            certification.ProjectId,
            certification.PaymentApplicationId,
            paymentApplication.ApplicationNumber,
            purchaseOrder.Id,
            purchaseOrder.PurchaseOrderNumber,
            supplier?.Name ?? "Unknown supplier",
            purchaseOrder.Currency,
            certification.CertificateNumber,
            certification.CertificateDate,
            certification.ClaimedAmount,
            certification.CertifiedAmount,
            certification.ClaimedAmount - certification.CertifiedAmount,
            certification.RetentionPercent,
            certification.RetentionAmount,
            certification.AdvancePaymentRecoveryAmount,
            certification.OtherDeductionAmount,
            certification.TotalDeductions,
            certification.PayableAmount,
            certification.Notes,
            certification.Status,
            creator?.Email ?? "Unknown user",
            certification.CreatedAt,
            certification.ApprovalRequestId,
            certification.OtherDeductions
                .OrderBy(deduction => deduction.Description)
                .Select(deduction => new CommercialDeductionDetails(
                    deduction.Id,
                    deduction.Description,
                    deduction.Amount))
                .ToArray());
    }
}

public sealed record ListCommercialCertificationsQuery(
    Guid ProjectId,
    CommercialCertificationStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListCommercialCertificationsHandler(
    ICommercialCertificationRepository certifications,
    IPaymentApplicationRepository paymentApplications,
    IPurchaseOrderRepository purchaseOrders,
    ISupplierRepository suppliers)
{
    public async Task<CommercialCertificationListResult> HandleAsync(
        ListCommercialCertificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await certifications.CountForProjectAsync(
            query.ProjectId,
            query.Status,
            cancellationToken);

        var certificationList = await certifications.ListForProjectAsync(
            query.ProjectId,
            query.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        var paymentApplicationIds = certificationList
            .Select(certification => certification.PaymentApplicationId)
            .Distinct()
            .ToArray();

        var applications = await paymentApplications.ListByIdsAsync(
            paymentApplicationIds,
            cancellationToken);

        var applicationsById = applications.ToDictionary(
            application => application.Id);

        var purchaseOrderIds = applications
            .Select(application => application.PurchaseOrderId)
            .Distinct()
            .ToArray();

        var orders = await purchaseOrders.ListByIdsAsync(
            purchaseOrderIds,
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

        return new CommercialCertificationListResult(
            certificationList.Select(certification =>
            {
                var application = applicationsById.GetValueOrDefault(
                    certification.PaymentApplicationId);

                var order = application is null
                    ? null
                    : ordersById.GetValueOrDefault(
                        application.PurchaseOrderId);

                var supplierName = order is null
                    ? "Unknown supplier"
                    : suppliersById.GetValueOrDefault(
                            order.SupplierId)?.Name
                        ?? "Unknown supplier";

                return new CommercialCertificationSummary(
                    certification.Id,
                    certification.CertificateNumber,
                    application?.ApplicationNumber
                        ?? "Unknown application",
                    order?.PurchaseOrderNumber ?? "Unknown PO",
                    supplierName,
                    certification.CertificateDate,
                    order?.Currency ?? "---",
                    certification.ClaimedAmount,
                    certification.CertifiedAmount,
                    certification.PayableAmount,
                    certification.Status);
            }).ToArray(),
            page,
            pageSize,
            totalCount);
    }
}

public sealed class ListCommercialCertificationApproversHandler(
    IUserDirectory users)
{
    public Task<IReadOnlyList<UserDirectoryEntry>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        users.ListActiveAsync(cancellationToken);
}
