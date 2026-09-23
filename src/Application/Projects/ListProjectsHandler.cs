using Domain.Projects;

namespace Application.Projects;

public sealed record ListProjectsRequest(
    string? Search = null,
    ProjectStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListProjectsHandler(
    IProjectRepository projects)
{
    public async Task<ProjectListResult> HandleAsync(
        ListProjectsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        var totalCount = await projects.CountAsync(
            search,
            request.Status,
            cancellationToken);

        var items = await projects.ListAsync(
            search,
            request.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return new ProjectListResult(
            items.Select(project => new ProjectSummary(
                    project.Id,
                    project.ProjectNumber,
                    project.Name,
                    project.Location,
                    project.StartDate,
                    project.TargetCompletionDate,
                    project.Status))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
