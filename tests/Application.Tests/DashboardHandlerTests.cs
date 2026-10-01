using Application.Common.Authentication;
using Application.Dashboard;
using Domain.DailyReports;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class DashboardHandlerTests
{
    [Fact]
    public async Task Portfolio_dashboard_passes_current_user_and_today()
    {
        var userId = Guid.NewGuid();
        var query = new FakeDashboardQueryService();
        var handler = new GetPortfolioDashboardHandler(
            query,
            new FakeCurrentUser(userId),
            new FixedTimeProvider(
                new DateTimeOffset(
                    2026,
                    10,
                    1,
                    8,
                    0,
                    0,
                    TimeSpan.Zero)));

        await handler.HandleAsync();

        Assert.Equal(userId, query.PortfolioUserId);
        Assert.Equal(
            new DateOnly(2026, 10, 1),
            query.PortfolioToday);
    }

    [Fact]
    public async Task Project_dashboard_passes_project_and_today()
    {
        var projectId = Guid.NewGuid();
        var query = new FakeDashboardQueryService();
        var handler = new GetProjectDashboardHandler(
            query,
            new FixedTimeProvider(
                new DateTimeOffset(
                    2026,
                    10,
                    1,
                    8,
                    0,
                    0,
                    TimeSpan.Zero)));

        await handler.HandleAsync(projectId);

        Assert.Equal(projectId, query.ProjectId);
        Assert.Equal(
            new DateOnly(2026, 10, 1),
            query.ProjectToday);
    }

    private sealed class FakeDashboardQueryService
        : IDashboardQueryService
    {
        public Guid? PortfolioUserId { get; private set; }

        public DateOnly PortfolioToday { get; private set; }

        public Guid? ProjectId { get; private set; }

        public DateOnly ProjectToday { get; private set; }

        public Task<PortfolioDashboardSnapshot> GetPortfolioAsync(
            Guid? userId,
            DateOnly today,
            CancellationToken cancellationToken = default)
        {
            PortfolioUserId = userId;
            PortfolioToday = today;

            return Task.FromResult(
                new PortfolioDashboardSnapshot(
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    []));
        }

        public Task<ProjectDashboardSnapshot?> GetProjectAsync(
            Guid projectId,
            DateOnly today,
            CancellationToken cancellationToken = default)
        {
            ProjectId = projectId;
            ProjectToday = today;

            return Task.FromResult<ProjectDashboardSnapshot?>(
                new ProjectDashboardSnapshot(
                    projectId,
                    "P-001",
                    "Project",
                    null,
                    ProjectStatus.Active,
                    new DateOnly(2026, 1, 1),
                    null,
                    false,
                    null,
                    null,
                    (DailyReportStatus?)null,
                    0,
                    0,
                    new DailyReportWorkflowCounts(0, 0, 0, 0, 0),
                    new DocumentWorkflowCounts(0, 0, 0),
                    new ApprovalWorkflowCounts(0, 0, 0, 0, 0, 0),
                    new ApprovalWorkflowCounts(0, 0, 0, 0, 0, 0),
                    new DeliveryWorkflowCounts(0, 0, 0, 0),
                    new ApprovalWorkflowCounts(0, 0, 0, 0, 0, 0),
                    new ApprovalWorkflowCounts(0, 0, 0, 0, 0, 0)));
        }
    }

    private sealed class FakeCurrentUser(Guid userId)
        : ICurrentUser
    {
        public ValueTask<CurrentUserInfo> GetAsync(
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(
                new CurrentUserInfo(
                    userId,
                    "dashboard@example.com",
                    true));
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
