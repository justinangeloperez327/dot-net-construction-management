using Application.Common.Authentication;
using Application.Common.Files;
using Application.DailyReports;
using Domain.DailyReports;
using Xunit;

namespace Application.Tests;

public sealed class DailyReportAttachmentHandlerTests
{
    [Fact]
    public async Task Upload_stores_file_and_metadata()
    {
        var report = CreateReport();
        var repository = new FakeRepository(report);
        var storage = new FakeFileStorage();
        var userId = Guid.NewGuid();

        var handler = new UploadDailyReportAttachmentHandler(
            repository,
            storage,
            new FakeCurrentUser(userId),
            TimeProvider.System);

        await using var content = new MemoryStream([1, 2, 3, 4]);

        var result = await handler.HandleAsync(
            report.Id,
            new UploadDailyReportAttachmentRequest(
                "site.jpg",
                "image/jpeg",
                content.Length,
                "Progress",
                content));

        Assert.True(result.Succeeded);
        Assert.Single(report.Attachments);
        Assert.Single(storage.Files);
        Assert.Equal(userId, report.Attachments.Single().UploadedByUserId);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Upload_rejects_disallowed_file_type()
    {
        var report = CreateReport();
        var storage = new FakeFileStorage();

        var handler = new UploadDailyReportAttachmentHandler(
            new FakeRepository(report),
            storage,
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        await using var content = new MemoryStream([1, 2, 3]);

        var result = await handler.HandleAsync(
            report.Id,
            new UploadDailyReportAttachmentRequest(
                "script.svg",
                "image/svg+xml",
                content.Length,
                null,
                content));

        Assert.False(result.Succeeded);
        Assert.Empty(storage.Files);
        Assert.Empty(report.Attachments);
    }

    [Fact]
    public async Task Delete_removes_metadata_and_stored_file()
    {
        var report = CreateReport();
        var attachmentId = Guid.NewGuid();
        var key = $"daily-reports/{report.Id:N}/{attachmentId:N}.pdf";

        report.AddAttachment(
            attachmentId,
            key,
            "report.pdf",
            "application/pdf",
            3,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null);

        var repository = new FakeRepository(report);
        var storage = new FakeFileStorage();
        storage.Files[key] = [1, 2, 3];

        var handler = new DeleteDailyReportAttachmentHandler(
            repository,
            storage);

        var result = await handler.HandleAsync(
            report.Id,
            attachmentId);

        Assert.True(result.Succeeded);
        Assert.Empty(report.Attachments);
        Assert.Empty(storage.Files);
        Assert.Equal(1, repository.SaveCount);
    }

    private static DailyReport CreateReport() =>
        DailyReport.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 24),
            Guid.NewGuid(),
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
                    "uploader@example.com",
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
            Stream? stream = Files.TryGetValue(storageKey, out var content)
                ? new MemoryStream(content, writable: false)
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

    private sealed class FakeRepository(DailyReport report)
        : IDailyReportRepository
    {
        public int SaveCount { get; private set; }

        public Task<DailyReport?> GetByIdAsync(
            Guid reportId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<DailyReport?>(
                report.Id == reportId ? report : null);

        public Task<bool> ExistsForProjectDateAsync(
            Guid projectId,
            DateOnly reportDate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<DailyReport>> ListForProjectAsync(
            Guid projectId,
            DailyReportStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DailyReport>>([]);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            DailyReportStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            DailyReport report,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
