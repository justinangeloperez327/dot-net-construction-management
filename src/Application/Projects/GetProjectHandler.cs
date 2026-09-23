namespace Application.Projects;

public sealed class GetProjectHandler(
    IProjectRepository projects)
{
    public async Task<ProjectDetails?> HandleAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        return project is null
            ? null
            : new ProjectDetails(
                project.Id,
                project.ProjectNumber,
                project.Name,
                project.Location,
                project.StartDate,
                project.TargetCompletionDate,
                project.Description,
                project.Status);
    }
}
