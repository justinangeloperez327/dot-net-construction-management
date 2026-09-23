using Domain.Projects;

namespace Application.Projects;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectMember>> ListAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ProjectMember member,
        CancellationToken cancellationToken = default);

    void Remove(ProjectMember member);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
