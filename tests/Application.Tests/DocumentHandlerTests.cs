using Application.Common.Authentication;
using Application.Documents;
using Application.Projects;
using Domain.Documents;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class DocumentHandlerTests
{
    [Fact]
    public async Task Create_rejects_duplicate_number_in_same_project()
    {
        var project = CreateProject();
        var repository = new FakeDocumentRepository
        {
            NumberExists = true
        };

        var handler = new CreateDocumentHandler(
            repository,
            new FakeProjectRepository(project),
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateDocumentRequest(
                project.Id,
                "DRW-001",
                "Ground Floor Plan",
                "Drawing",
                "Architectural",
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Create_rejects_closed_project()
    {
        var project = CreateProject();
        project.Close();

        var repository = new FakeDocumentRepository();

        var handler = new CreateDocumentHandler(
            repository,
            new FakeProjectRepository(project),
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateDocumentRequest(
                project.Id,
                "DRW-001",
                "Ground Floor Plan",
                null,
                null,
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Create_records_authenticated_creator()
    {
        var userId = Guid.NewGuid();
        var project = CreateProject();
        var repository = new FakeDocumentRepository();

        var handler = new CreateDocumentHandler(
            repository,
            new FakeProjectRepository(project),
            new FakeCurrentUser(userId),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateDocumentRequest(
                project.Id,
                "DRW-001",
                "Ground Floor Plan",
                null,
                null,
                null,
                null));

        Assert.True(result.Succeeded);
        Assert.Single(repository.Added);
        Assert.Equal(
            userId,
            repository.Added[0].CreatedByUserId);
    }

    private static Project CreateProject() =>
        Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 1),
            null,
            null);

    private sealed class FakeCurrentUser(Guid userId)
        : ICurrentUser
    {
        public ValueTask<CurrentUserInfo> GetAsync(
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(
                new CurrentUserInfo(
                    userId,
                    "user@example.com",
                    true));
    }

    private sealed class FakeDocumentRepository
        : IDocumentRepository
    {
        public bool NumberExists { get; set; }

        public List<Document> Added { get; } = [];

        public Task<Document?> GetByIdAsync(
            Guid documentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Document?>(null);

        public Task<bool> DocumentNumberExistsAsync(
            Guid projectId,
            string documentNumber,
            Guid? excludingDocumentId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NumberExists);

        public Task<IReadOnlyList<Document>> ListForProjectAsync(
            Guid projectId,
            string? search,
            DocumentStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Document>>([]);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            string? search,
            DocumentStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Document document,
            CancellationToken cancellationToken = default)
        {
            Added.Add(document);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
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
}
