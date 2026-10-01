using Application.Common.Authentication;

namespace Application.Dashboard;

public sealed class GetPortfolioDashboardHandler(
    IDashboardQueryService dashboard,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<PortfolioDashboardSnapshot> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var user = await currentUser.GetAsync(cancellationToken);
        var today = DateOnly.FromDateTime(
            timeProvider.GetUtcNow().UtcDateTime);

        return await dashboard.GetPortfolioAsync(
            user.UserId,
            today,
            cancellationToken);
    }
}

public sealed class GetProjectDashboardHandler(
    IDashboardQueryService dashboard,
    TimeProvider timeProvider)
{
    public Task<ProjectDashboardSnapshot?> HandleAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(
            timeProvider.GetUtcNow().UtcDateTime);

        return dashboard.GetProjectAsync(
            projectId,
            today,
            cancellationToken);
    }
}
