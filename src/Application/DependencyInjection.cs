using Application.Approvals;
using Application.Clients;
using Application.DailyReports;
using Application.Documents;
using Application.Projects;
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

        return services;
    }
}
