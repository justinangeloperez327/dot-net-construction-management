using Domain.Projects;

namespace Application.Projects;

public sealed record ProjectSummary(
    Guid Id,
    string ProjectNumber,
    string Name,
    string? Location,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    ProjectStatus Status);

public sealed record ProjectDetails(
    Guid Id,
    string ProjectNumber,
    string Name,
    string? Location,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    string? Description,
    ProjectStatus Status);

public sealed record ProjectListResult(
    IReadOnlyList<ProjectSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record ProjectActionResult(
    bool Succeeded,
    Guid? ProjectId,
    IReadOnlyList<string> Errors)
{
    public static ProjectActionResult Success(Guid projectId) =>
        new(true, projectId, []);

    public static ProjectActionResult Failure(params string[] errors) =>
        new(false, null, errors);
}
