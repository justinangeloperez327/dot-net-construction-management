using Domain.Projects;

namespace Application.Projects;

public sealed record CreateProjectRequest(
    string ProjectNumber,
    string Name,
    string? Location,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    string? Description);

public sealed class CreateProjectHandler(
    IProjectRepository projects)
{
    public async Task<ProjectActionResult> HandleAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectNumber = request.ProjectNumber.Trim();

        if (await projects.ProjectNumberExistsAsync(
                projectNumber,
                cancellationToken: cancellationToken))
        {
            return ProjectActionResult.Failure(
                "A project with this project number already exists.");
        }

        try
        {
            var project = Project.Create(
                projectNumber,
                request.Name,
                request.Location,
                request.StartDate,
                request.TargetCompletionDate,
                request.Description);

            await projects.AddAsync(project, cancellationToken);
            await projects.SaveChangesAsync(cancellationToken);

            return ProjectActionResult.Success(project.Id);
        }
        catch (ArgumentException exception)
        {
            return ProjectActionResult.Failure(exception.Message);
        }
    }
}
