using Application.Clients;

namespace Application.Projects;

public sealed class GetProjectHandler(
    IProjectRepository projects,
    IClientRepository clients)
{
    public async Task<ProjectDetails?> HandleAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            return null;
        }

        string? clientName = null;

        if (project.ClientId is Guid clientId)
        {
            var client = await clients.GetByIdAsync(
                clientId,
                cancellationToken);

            clientName = client?.Name;
        }

        return new ProjectDetails(
            project.Id,
            project.ProjectNumber,
            project.Name,
            project.ClientId,
            clientName,
            project.Location,
            project.StartDate,
            project.TargetCompletionDate,
            project.Description,
            project.Status);
    }
}
