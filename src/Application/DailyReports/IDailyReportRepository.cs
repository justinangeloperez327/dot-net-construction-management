using Domain.DailyReports;

namespace Application.DailyReports;

public interface IDailyReportRepository
{
    Task<DailyReport?> GetByIdAsync(
        Guid reportId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForProjectDateAsync(
        Guid projectId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyReport>> ListForProjectAsync(
        Guid projectId,
        DailyReportStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        DailyReportStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        DailyReport report,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
