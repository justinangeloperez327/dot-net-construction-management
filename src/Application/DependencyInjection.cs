using Application.Clients;
using Application.DailyReports;
using Application.Projects;
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

        return services;
    }
}
