using Application.Common.Authentication;
using Application.Common.Files;
using Application.Documents;
using Application.Projects;
using Domain.Documents;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class DocumentRevisionHandlerTests
{
    [Fact]
    public async Task Create_revision_stores_file_and_metadata()
    {
        var document = CreateDocument();
        var repository = new FakeDocumentRepository(document);
        var storage = new FakeFileStorage();
        var userId = Guid.NewGuid();

        var handler = new CreateDocumentRevisionHandler(
            repository,
            new FakeProjectRepository(CreateProject()),
            storage,
            new FakeCurrentUser(userId),
            TimeProvider.System);

        await using var content = new MemoryStream([1, 2, 3, 4]);

        var result = await handler.HandleAsync(
            document.Id,
            new CreateDocumentRevisionRequest(
                "00",
                "Initial issue",
                "drawing.pdf",
                "application/pdf",
                content.Length,
                content));

        Assert.True(result.Succeeded);
        Assert.Single(document.Revisions);
        Assert.Single(storage.Files);

        var revision = document.Revisions.Single();

        Assert.Equal("00", revision.RevisionCode);
        Assert.Equal(userId, revision.CreatedByUserId);
        Assert.Equal(DocumentRevisionStatus.Draft, revision.Status);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Create_revision_rejects_disallowed_file_type()
    {
        var document = CreateDocument();
        var storage = new FakeFileStorage();

        var handler = new CreateDocumentRevisionHandler(
            new FakeDocumentRepository(document),
            new FakeProjectRepository(CreateProject()),
            storage,
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        await using var content = new MemoryStream([1, 2, 3]);

        var result = await handler.HandleAsync(
            document.Id,
            new CreateDocumentRevisionRequest(
                "00",
                null,
                "drawing.svg",
                "image/svg+xml",
                content.Length,
                content));

        Assert.False(result.Succeeded);
        Assert.Empty(document.Revisions);
        Assert.Empty(storage.Files);
    }

    [Fact]
    public async Task Submit_revision_records_submitter()
    {
        var document = CreateDocument();
        var revision = AddRevision(document, "00");
        var submitterId = Guid.NewGuid();

        var handler = new SubmitDocumentRevisionHandler(
            new FakeDocumentRepository(document),
            new FakeProjectRepository(CreateProject()),
            new FakeCurrentUser(submitterId),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            document.Id,
            revision.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(
            DocumentRevisionStatus.Submitted,
            revision.Status);
        Assert.Equal(
            submitterId,
            revision.SubmittedByUserId);
    }

    private static Document CreateDocument() =>
        Document.Create(
            Guid.NewGuid(),
            "DRW-001",
            "Ground Floor Plan",
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

    private static DocumentRevision AddRevision(
        Document document,
        string revisionCode)
    {
        var id = Guid.NewGuid();

        return document.AddRevision(
            id,
            revisionCode,
            null,
            $"documents/{document.Id:N}/revisions/{id:N}.pdf",
            "drawing.pdf",
            "application/pdf",
            4,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
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

    private sealed class FakeFileStorage : IFileStorage
    {
        public Dictionary<string, byte[]> Files { get; } = [];

        public async Task WriteAsync(
            string storageKey,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            await using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            Files.Add(storageKey, buffer.ToArray());
        }

        public Task<Stream?> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            Stream? stream = Files.TryGetValue(
                storageKey,
                out var bytes)
                ? new MemoryStream(bytes, writable: false)
                : null;

            return Task.FromResult(stream);
        }

        public Task DeleteIfExistsAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            Files.Remove(storageKey);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDocumentRepository(Document document)
        : IDocumentRepository
    {
        public int SaveCount { get; private set; }

        public Task<Document?> GetByIdAsync(
            Guid documentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Document?>(
                document.Id == documentId ? document : null);

        public Task<bool> DocumentNumberExistsAsync(
            Guid projectId,
            string documentNumber,
            Guid? excludingDocumentId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

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
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProjectRepository(Project project)
        : IProjectRepository
    {
        public Task<Project?> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Project?>(project);

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
