using Application.Projects;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ProjectRepository(
    ApplicationDbContext dbContext)
    : IProjectRepository
{
    public Task<Project?> GetByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Projects
            .SingleOrDefaultAsync(
                project => project.Id == projectId,
                cancellationToken);
    }

    public Task<bool> ProjectNumberExistsAsync(
        string projectNumber,
        Guid? excludingProjectId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Projects.AnyAsync(
            project =>
                project.ProjectNumber == projectNumber &&
                (!excludingProjectId.HasValue ||
                 project.Id != excludingProjectId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(
        string? search,
        ProjectStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(
                dbContext.Projects.AsNoTracking(),
                search,
                status)
            .OrderBy(project => project.ProjectNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        ProjectStatus? status,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilter(
                dbContext.Projects.AsNoTracking(),
                search,
                status)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Projects.AddAsync(
            project,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Project> ApplyFilter(
        IQueryable<Project> query,
        string? search,
        ProjectStatus? status)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(project =>
                project.ProjectNumber.Contains(search) ||
                project.Name.Contains(search) ||
                (project.Location != null &&
                 project.Location.Contains(search)));
        }

        if (status is not null)
        {
            query = query.Where(
                project => project.Status == status);
        }

        return query;
    }
}
