using Application.Common.Files;
using Application.DailyReports;
using Domain.DailyReports;
using Infrastructure.Files;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class FileStorageAttachmentRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FileStorageAttachmentRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Attachment_services_and_mapping_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IFileStorage>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<UploadDailyReportAttachmentHandler>());

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(DailyReportAttachment));

        Assert.NotNull(entity);
        Assert.Equal(
            "DailyReportAttachments",
            entity.GetTableName());
    }

    [Fact]
    public async Task File_system_storage_round_trips_and_blocks_path_escape()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"cpm-storage-{Guid.NewGuid():N}");

        try
        {
            var storage = new FileSystemFileStorage(
                new FileStorageOptions(root));

            await using var content = new MemoryStream([1, 2, 3, 4]);

            await storage.WriteAsync(
                "daily-reports/report/file.pdf",
                content);

            await using var read = await storage.OpenReadAsync(
                "daily-reports/report/file.pdf");

            Assert.NotNull(read);

            await using var copy = new MemoryStream();
            await read.CopyToAsync(copy);

            Assert.Equal(
                new byte[] { 1, 2, 3, 4 },
                copy.ToArray());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                storage.WriteAsync(
                    "../escape.pdf",
                    new MemoryStream([1])));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
