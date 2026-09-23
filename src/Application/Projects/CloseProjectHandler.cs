namespace Application.Projects;

public sealed class CloseProjectHandler(
    IProjectRepository projects)
{
    public async Task<ProjectActionResult> HandleAsync(
        Guid projectId,
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

        project.Close();

        await projects.SaveChangesAsync(cancellationToken);

        return ProjectActionResult.Success(project.Id);
    }
}
