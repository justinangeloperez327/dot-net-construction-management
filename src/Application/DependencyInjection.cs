using Application.Approvals;
using Application.Auditing;
using Application.Clients;
using Application.Commercial;
using Application.DailyReports;
using Application.Dashboard;
using Application.Deliveries;
using Application.Documents;
using Application.PaymentApplications;
using Application.Projects;
using Application.Reporting;
using Application.PurchaseOrders;
using Application.PurchaseRequests;
using Application.Suppliers;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<CreateClientHandler>();
        services.AddScoped<UpdateClientHandler>();
        services.AddScoped<SetClientActiveHandler>();
        services.AddScoped<GetClientHandler>();
        services.AddScoped<ListClientsHandler>();
        services.AddScoped<ListActiveClientsHandler>();

        services.AddScoped<CreateProjectHandler>();
        services.AddScoped<UpdateProjectHandler>();
        services.AddScoped<CloseProjectHandler>();
        services.AddScoped<GetProjectHandler>();
        services.AddScoped<ListProjectsHandler>();
        services.AddScoped<AssignProjectClientHandler>();

        services.AddScoped<AddProjectMemberHandler>();
        services.AddScoped<UpdateProjectMemberHandler>();
        services.AddScoped<RemoveProjectMemberHandler>();
        services.AddScoped<ListProjectMembersHandler>();
        services.AddScoped<ListAssignableUsersHandler>();

        services.AddScoped<CreateDailyReportHandler>();
        services.AddScoped<UpdateDailyReportHandler>();
        services.AddScoped<AddDailyReportActivityHandler>();
        services.AddScoped<UpdateDailyReportActivityHandler>();
        services.AddScoped<RemoveDailyReportActivityHandler>();
        services.AddScoped<SubmitDailyReportHandler>();
        services.AddScoped<ReviewDailyReportHandler>();
        services.AddScoped<GetDailyReportHandler>();
        services.AddScoped<ListDailyReportsHandler>();

        services.AddScoped<AddManpowerEntryHandler>();
        services.AddScoped<UpdateManpowerEntryHandler>();
        services.AddScoped<RemoveManpowerEntryHandler>();
        services.AddScoped<AddEquipmentEntryHandler>();
        services.AddScoped<UpdateEquipmentEntryHandler>();
        services.AddScoped<RemoveEquipmentEntryHandler>();
        services.AddScoped<AddSiteIssueHandler>();
        services.AddScoped<UpdateSiteIssueHandler>();
        services.AddScoped<RemoveSiteIssueHandler>();

        services.AddScoped<UploadDailyReportAttachmentHandler>();
        services.AddScoped<DeleteDailyReportAttachmentHandler>();
        services.AddScoped<GetDailyReportAttachmentFileHandler>();

        services.AddScoped<CreateDocumentHandler>();
        services.AddScoped<UpdateDocumentHandler>();
        services.AddScoped<SetDocumentArchivedHandler>();
        services.AddScoped<GetDocumentHandler>();
        services.AddScoped<ListDocumentsHandler>();
        services.AddScoped<CreateDocumentRevisionHandler>();
        services.AddScoped<SubmitDocumentRevisionHandler>();
        services.AddScoped<ReviewDocumentRevisionHandler>();
        services.AddScoped<GetDocumentRevisionFileHandler>();

        services.AddScoped<CreateApprovalRequestHandler>();
        services.AddScoped<DecideApprovalStepHandler>();
        services.AddScoped<CancelApprovalRequestHandler>();
        services.AddScoped<GetApprovalRequestHandler>();
        services.AddScoped<ListApprovalRequestsHandler>();

        services.AddScoped<CreateSupplierHandler>();
        services.AddScoped<UpdateSupplierHandler>();
        services.AddScoped<SetSupplierActiveHandler>();
        services.AddScoped<GetSupplierHandler>();
        services.AddScoped<ListSuppliersHandler>();
        services.AddScoped<ListActiveSuppliersHandler>();

        services.AddScoped<CreatePurchaseRequestHandler>();
        services.AddScoped<UpdatePurchaseRequestHandler>();
        services.AddScoped<AddPurchaseRequestItemHandler>();
        services.AddScoped<UpdatePurchaseRequestItemHandler>();
        services.AddScoped<RemovePurchaseRequestItemHandler>();
        services.AddScoped<SubmitPurchaseRequestHandler>();
        services.AddScoped<CancelPurchaseRequestHandler>();
        services.AddScoped<GetPurchaseRequestHandler>();
        services.AddScoped<ListPurchaseRequestsHandler>();
        services.AddScoped<ListPurchaseRequestApproversHandler>();
        services.AddScoped<
            IApprovalSubjectOutcomeHandler,
            PurchaseRequestApprovalOutcomeHandler>();

        services.AddScoped<CreatePurchaseOrderHandler>();
        services.AddScoped<UpdatePurchaseOrderHandler>();
        services.AddScoped<UpdatePurchaseOrderItemHandler>();
        services.AddScoped<RemovePurchaseOrderItemHandler>();
        services.AddScoped<SubmitPurchaseOrderHandler>();
        services.AddScoped<CancelPurchaseOrderHandler>();
        services.AddScoped<GetPurchaseOrderHandler>();
        services.AddScoped<ListPurchaseOrdersHandler>();
        services.AddScoped<ListPurchaseOrderApproversHandler>();
        services.AddScoped<
            IApprovalSubjectOutcomeHandler,
            PurchaseOrderApprovalOutcomeHandler>();

        services.AddScoped<CreateDeliveryHandler>();
        services.AddScoped<UpdateDeliveryHandler>();
        services.AddScoped<AddDeliveryItemHandler>();
        services.AddScoped<UpdateDeliveryItemHandler>();
        services.AddScoped<RemoveDeliveryItemHandler>();
        services.AddScoped<ReceiveDeliveryHandler>();
        services.AddScoped<CancelDeliveryHandler>();
        services.AddScoped<GetDeliveryHandler>();
        services.AddScoped<ListDeliveriesHandler>();
        services.AddScoped<ListAvailableDeliveryItemsHandler>();

        services.AddScoped<CreatePaymentApplicationHandler>();
        services.AddScoped<UpdatePaymentApplicationHandler>();
        services.AddScoped<AddPaymentApplicationItemHandler>();
        services.AddScoped<UpdatePaymentApplicationItemHandler>();
        services.AddScoped<RemovePaymentApplicationItemHandler>();
        services.AddScoped<SubmitPaymentApplicationHandler>();
        services.AddScoped<CancelPaymentApplicationHandler>();
        services.AddScoped<GetPaymentApplicationHandler>();
        services.AddScoped<ListPaymentApplicationsHandler>();
        services.AddScoped<ListAvailablePaymentApplicationItemsHandler>();
        services.AddScoped<ListPaymentApplicationApproversHandler>();
        services.AddScoped<
            IApprovalSubjectOutcomeHandler,
            PaymentApplicationApprovalOutcomeHandler>();

        services.AddScoped<CreateCommercialCertificationHandler>();
        services.AddScoped<UpdateCommercialCertificationHandler>();
        services.AddScoped<AddCommercialDeductionHandler>();
        services.AddScoped<UpdateCommercialDeductionHandler>();
        services.AddScoped<RemoveCommercialDeductionHandler>();
        services.AddScoped<SubmitCommercialCertificationHandler>();
        services.AddScoped<CancelCommercialCertificationHandler>();
        services.AddScoped<GetCommercialCertificationHandler>();
        services.AddScoped<ListCommercialCertificationsHandler>();
        services.AddScoped<ListCommercialCertificationApproversHandler>();
        services.AddScoped<
            IApprovalSubjectOutcomeHandler,
            CommercialCertificationApprovalOutcomeHandler>();

        services.AddScoped<GetPortfolioDashboardHandler>();
        services.AddScoped<GetProjectDashboardHandler>();

        services.AddScoped<GetReportingProjectsHandler>();
        services.AddScoped<GetPortfolioReportHandler>();
        services.AddScoped<GetDailySiteReportHandler>();
        services.AddScoped<GetProcurementReportHandler>();
        services.AddScoped<GetCommercialReportHandler>();
        services.AddScoped<ExportReportCsvHandler>();

        services.AddScoped<ListAuditTrailHandler>();
        services.AddScoped<ListAuditEntityTypesHandler>();

        return services;
    }
}
