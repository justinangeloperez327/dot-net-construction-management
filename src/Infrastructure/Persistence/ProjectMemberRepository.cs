using Application.Projects;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ProjectMemberRepository(
    ApplicationDbContext dbContext)
    : IProjectMemberRepository
{
    public Task<ProjectMember?> GetAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProjectMembers
            .SingleOrDefaultAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId,
                cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProjectMembers
            .AnyAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectMember>> ListAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectMembers
            .AsNoTracking()
            .Where(member => member.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ProjectMember member,
        CancellationToken cancellationToken = default)
    {
        await dbContext.ProjectMembers.AddAsync(
            member,
            cancellationToken);
    }

    public void Remove(ProjectMember member)
    {
        dbContext.ProjectMembers.Remove(member);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
