using Application.Clients;
using Application.Projects;
using Application.Users;
using Domain.Clients;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class ClientProjectMemberHandlerTests
{
    [Fact]
    public async Task Assign_client_rejects_inactive_client()
    {
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 23),
            null,
            null);

        var client = Client.Create(
            "Owner",
            null,
            null,
            null,
            null);

        client.SetActive(false);

        var projects = new FakeProjectRepository(project);
        var clients = new FakeClientRepository(client);
        var handler = new AssignProjectClientHandler(
            projects,
            clients);

        var result = await handler.HandleAsync(
            project.Id,
            client.Id);

        Assert.False(result.Succeeded);
        Assert.Null(project.ClientId);
    }

    [Fact]
    public async Task Add_member_rejects_duplicate_assignment()
    {
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 23),
            null,
            null);

        var userId = Guid.NewGuid();

        var handler = new AddProjectMemberHandler(
            new FakeProjectRepository(project),
            new FakeProjectMemberRepository(exists: true),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    userId,
                    "engineer@example.com",
                    true)));

        var result = await handler.HandleAsync(
            project.Id,
            new AddProjectMemberRequest(
                userId,
                "Site Engineer"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Add_member_accepts_active_user()
    {
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 23),
            null,
            null);

        var userId = Guid.NewGuid();
        var members = new FakeProjectMemberRepository();

        var handler = new AddProjectMemberHandler(
            new FakeProjectRepository(project),
            members,
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    userId,
                    "engineer@example.com",
                    true)));

        var result = await handler.HandleAsync(
            project.Id,
            new AddProjectMemberRequest(
                userId,
                "Site Engineer"));

        Assert.True(result.Succeeded);
        Assert.Single(members.Added);
        Assert.Equal(1, members.SaveCount);
    }

    private sealed class FakeProjectRepository(Project project)
        : IProjectRepository
    {
        public Task<Project?> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Project?>(
                project.Id == projectId ? project : null);

        public Task<bool> ProjectNumberExistsAsync(
            string projectNumber,
            Guid? excludingProjectId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<Project>> ListAsync(
            string? search,
            ProjectStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Project>>([]);

        public Task<int> CountAsync(
            string? search,
            ProjectStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeClientRepository(Client client)
        : IClientRepository
    {
        public Task<Client?> GetByIdAsync(
            Guid clientId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Client?>(
                client.Id == clientId ? client : null);

        public Task<IReadOnlyList<Client>> ListAsync(
            string? search,
            bool? isActive,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Client>>([]);

        public Task<IReadOnlyList<Client>> ListActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Client>>([]);

        public Task<int> CountAsync(
            string? search,
            bool? isActive,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Client client,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeProjectMemberRepository(
        bool exists = false)
        : IProjectMemberRepository
    {
        public List<ProjectMember> Added { get; } = [];

        public int SaveCount { get; private set; }

        public Task<ProjectMember?> GetAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ProjectMember?>(null);

        public Task<bool> ExistsAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);

        public Task<IReadOnlyList<ProjectMember>> ListAsync(
            Guid projectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ProjectMember>>([]);

        public Task AddAsync(
            ProjectMember member,
            CancellationToken cancellationToken = default)
        {
            Added.Add(member);
            return Task.CompletedTask;
        }

        public void Remove(ProjectMember member)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserDirectory(
        UserDirectoryEntry user)
        : IUserDirectory
    {
        public Task<UserDirectoryEntry?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<UserDirectoryEntry?>(
                user.Id == userId ? user : null);

        public Task<IReadOnlyList<UserDirectoryEntry>> ListActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<UserDirectoryEntry>>(
                user.IsActive ? [user] : []);

        public Task<IReadOnlyList<UserDirectoryEntry>> ListByIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<UserDirectoryEntry>>(
                userIds.Contains(user.Id) ? [user] : []);
    }
}
