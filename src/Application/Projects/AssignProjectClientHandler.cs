using Application.Clients;

namespace Application.Projects;

public sealed class AssignProjectClientHandler(
    IProjectRepository projects,
    IClientRepository clients)
{
    public async Task<ProjectActionResult> HandleAsync(
        Guid projectId,
        Guid? clientId,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            return ProjectActionResult.Failure(
                "Project was not found.");
        }

        if (clientId is not null)
        {
            var client = await clients.GetByIdAsync(
                clientId.Value,
                cancellationToken);

            if (client is null || !client.IsActive)
            {
                return ProjectActionResult.Failure(
                    "Select an active client.");
            }
        }

        try
        {
            project.AssignClient(clientId);

            await projects.SaveChangesAsync(cancellationToken);

            return ProjectActionResult.Success(project.Id);
        }
        catch (ArgumentException exception)
        {
            return ProjectActionResult.Failure(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return ProjectActionResult.Failure(exception.Message);
        }
    }
}
