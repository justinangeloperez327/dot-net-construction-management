using Application.DailyReports;
using Domain.DailyReports;
using Xunit;

namespace Application.Tests;

public sealed class DailyReportResourceHandlerTests
{
    [Fact]
    public async Task Add_manpower_persists_valid_entry()
    {
        var report = CreateReport();
        var repository = new FakeRepository(report);
        var handler = new AddManpowerEntryHandler(repository);

        var result = await handler.HandleAsync(
            report.Id,
            new SaveManpowerEntryRequest(
                "Electrician",
                "Main Contractor",
                4,
                32,
                null));

        Assert.True(result.Succeeded);
        Assert.Single(report.ManpowerEntries);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Add_equipment_rejects_invalid_quantity()
    {
        var report = CreateReport();
        var handler = new AddEquipmentEntryHandler(
            new FakeRepository(report));

        var result = await handler.HandleAsync(
            report.Id,
            new SaveEquipmentEntryRequest(
                "Generator",
                null,
                0,
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(report.EquipmentEntries);
    }

    [Fact]
    public async Task Add_site_issue_persists_valid_issue()
    {
        var report = CreateReport();
        var repository = new FakeRepository(report);
        var handler = new AddSiteIssueHandler(repository);

        var result = await handler.HandleAsync(
            report.Id,
            new SaveSiteIssueRequest(
                "Material delay",
                "Required material was not delivered.",
                "Supplier contacted.",
                SiteIssueStatus.Open));

        Assert.True(result.Succeeded);
        Assert.Single(report.SiteIssues);
    }

    private static DailyReport CreateReport() =>
        DailyReport.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 24),
            Guid.NewGuid(),
            null,
            null);

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
