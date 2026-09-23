using Domain.Projects;

namespace Application.Projects;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<bool> ProjectNumberExistsAsync(
        string projectNumber,
        Guid? excludingProjectId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> ListAsync(
        string? search,
        ProjectStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        string? search,
        ProjectStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
