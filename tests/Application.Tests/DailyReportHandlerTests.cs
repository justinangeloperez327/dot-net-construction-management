using Application.Common.Authentication;
using Application.DailyReports;
using Application.Projects;
using Domain.DailyReports;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class DailyReportHandlerTests
{
    [Fact]
    public async Task Create_rejects_duplicate_project_date()
    {
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 1),
            null,
            null);

        var reports = new FakeReportRepository
        {
            ExistsForDate = true
        };

        var handler = new CreateDailyReportHandler(
            reports,
            new FakeProjectRepository(project),
            new FakeCurrentUser(Guid.NewGuid()));

        var result = await handler.HandleAsync(
            new CreateDailyReportRequest(
                project.Id,
                new DateOnly(2026, 9, 24),
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(reports.Added);
    }

    [Fact]
    public async Task Create_records_authenticated_preparer()
    {
        var userId = Guid.NewGuid();
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 1),
            null,
            null);

        var reports = new FakeReportRepository();

        var handler = new CreateDailyReportHandler(
            reports,
            new FakeProjectRepository(project),
            new FakeCurrentUser(userId));

        var result = await handler.HandleAsync(
            new CreateDailyReportRequest(
                project.Id,
                new DateOnly(2026, 9, 24),
                "Clear",
                null));

        Assert.True(result.Succeeded);
        Assert.Single(reports.Added);
        Assert.Equal(userId, reports.Added[0].PreparedByUserId);
    }

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

    private sealed class FakeReportRepository
        : IDailyReportRepository
    {
        public bool ExistsForDate { get; set; }

        public List<DailyReport> Added { get; } = [];

        public Task<DailyReport?> GetByIdAsync(
            Guid reportId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<DailyReport?>(null);

        public Task<bool> ExistsForProjectDateAsync(
            Guid projectId,
            DateOnly reportDate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistsForDate);

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
            CancellationToken cancellationToken = default)
        {
            Added.Add(report);
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
