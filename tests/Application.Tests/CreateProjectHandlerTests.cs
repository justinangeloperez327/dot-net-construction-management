using Application.Projects;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class CreateProjectHandlerTests
{
    [Fact]
    public async Task Create_rejects_duplicate_project_number()
    {
        var repository = new FakeProjectRepository
        {
            ProjectNumberExists = true
        };

        var handler = new CreateProjectHandler(repository);

        var result = await handler.HandleAsync(
            new CreateProjectRequest(
                "P-001",
                "Tower Project",
                null,
                new DateOnly(2026, 9, 23),
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Create_persists_valid_project()
    {
        var repository = new FakeProjectRepository();
        var handler = new CreateProjectHandler(repository);

        var result = await handler.HandleAsync(
            new CreateProjectRequest(
                "P-001",
                "Tower Project",
                "Abu Dhabi",
                new DateOnly(2026, 9, 23),
                null,
                null));

        Assert.True(result.Succeeded);
        Assert.Single(repository.Added);
        Assert.Equal(1, repository.SaveCount);
    }

    private sealed class FakeProjectRepository
        : IProjectRepository
    {
        public bool ProjectNumberExists { get; set; }

        public List<Project> Added { get; } = [];

        public int SaveCount { get; private set; }

        public Task<Project?> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Project?>(null);

        public Task<bool> ProjectNumberExistsAsync(
            string projectNumber,
            Guid? excludingProjectId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ProjectNumberExists);

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
            CancellationToken cancellationToken = default)
        {
            Added.Add(project);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
