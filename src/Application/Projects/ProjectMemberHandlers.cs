using Application.Users;
using Domain.Projects;

namespace Application.Projects;

public sealed record AddProjectMemberRequest(
    Guid UserId,
    string? Responsibility);

public sealed class AddProjectMemberHandler(
    IProjectRepository projects,
    IProjectMemberRepository members,
    IUserDirectory users)
{
    public async Task<ProjectMemberActionResult> HandleAsync(
        Guid projectId,
        AddProjectMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            return ProjectMemberActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return ProjectMemberActionResult.Failure(
                "Members cannot be changed on a closed project.");
        }

        var user = await users.GetAsync(
            request.UserId,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ProjectMemberActionResult.Failure(
                "Select an active application user.");
        }

        if (await members.ExistsAsync(
                projectId,
                request.UserId,
                cancellationToken))
        {
            return ProjectMemberActionResult.Failure(
                "This user is already assigned to the project.");
        }

        try
        {
            var member = ProjectMember.Assign(
                projectId,
                request.UserId,
                request.Responsibility);

            await members.AddAsync(member, cancellationToken);
            await members.SaveChangesAsync(cancellationToken);

            return ProjectMemberActionResult.Success();
        }
        catch (ArgumentException exception)
        {
            return ProjectMemberActionResult.Failure(exception.Message);
        }
    }
}

public sealed class UpdateProjectMemberHandler(
    IProjectRepository projects,
    IProjectMemberRepository members)
{
    public async Task<ProjectMemberActionResult> HandleAsync(
        Guid projectId,
        Guid userId,
        string? responsibility,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            return ProjectMemberActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return ProjectMemberActionResult.Failure(
                "Members cannot be changed on a closed project.");
        }

        var member = await members.GetAsync(
            projectId,
            userId,
            cancellationToken);

        if (member is null)
        {
            return ProjectMemberActionResult.Failure(
                "Project member was not found.");
        }

        member.UpdateResponsibility(responsibility);

        await members.SaveChangesAsync(cancellationToken);

        return ProjectMemberActionResult.Success();
    }
}

public sealed class RemoveProjectMemberHandler(
    IProjectRepository projects,
    IProjectMemberRepository members)
{
    public async Task<ProjectMemberActionResult> HandleAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
        {
            return ProjectMemberActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return ProjectMemberActionResult.Failure(
                "Members cannot be changed on a closed project.");
        }

        var member = await members.GetAsync(
            projectId,
            userId,
            cancellationToken);

        if (member is null)
        {
            return ProjectMemberActionResult.Failure(
                "Project member was not found.");
        }

        members.Remove(member);
        await members.SaveChangesAsync(cancellationToken);

        return ProjectMemberActionResult.Success();
    }
}

public sealed class ListProjectMembersHandler(
    IProjectMemberRepository members,
    IUserDirectory users)
{
    public async Task<IReadOnlyList<ProjectMemberDetails>> HandleAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await members.ListAsync(
            projectId,
            cancellationToken);

        if (assignments.Count == 0)
        {
            return [];
        }

        var directory = await users.ListByIdsAsync(
            assignments.Select(member => member.UserId).ToArray(),
            cancellationToken);

        var usersById = directory.ToDictionary(user => user.Id);

        return assignments
            .Where(member => usersById.ContainsKey(member.UserId))
            .Select(member => new ProjectMemberDetails(
                member.UserId,
                usersById[member.UserId].Email,
                member.Responsibility))
            .OrderBy(member => member.Email)
            .ToArray();
    }
}

public sealed class ListAssignableUsersHandler(
    IUserDirectory users)
{
    public Task<IReadOnlyList<UserDirectoryEntry>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return users.ListActiveAsync(cancellationToken);
    }
}
