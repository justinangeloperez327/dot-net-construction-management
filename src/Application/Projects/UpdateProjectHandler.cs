namespace Application.Projects;

public sealed record UpdateProjectRequest(
    string ProjectNumber,
    string Name,
    string? Location,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    string? Description);

public sealed class UpdateProjectHandler(
    IProjectRepository projects)
{
    public async Task<ProjectActionResult> HandleAsync(
        Guid projectId,
        UpdateProjectRequest request,
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

        var projectNumber = request.ProjectNumber.Trim();

        if (await projects.ProjectNumberExistsAsync(
                projectNumber,
                projectId,
                cancellationToken))
        {
            return ProjectActionResult.Failure(
                "A project with this project number already exists.");
        }

        try
        {
            project.Update(
                projectNumber,
                request.Name,
                request.Location,
                request.StartDate,
                request.TargetCompletionDate,
                request.Description);

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
