namespace Application.Dashboard;

public interface IDashboardQueryService
{
    Task<PortfolioDashboardSnapshot> GetPortfolioAsync(
        Guid? userId,
        DateOnly today,
        CancellationToken cancellationToken = default);

    Task<ProjectDashboardSnapshot?> GetProjectAsync(
        Guid projectId,
        DateOnly today,
        CancellationToken cancellationToken = default);
}
