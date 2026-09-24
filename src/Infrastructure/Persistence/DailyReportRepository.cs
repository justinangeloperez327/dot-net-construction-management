using Application.DailyReports;
using Domain.DailyReports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DailyReportRepository(
    ApplicationDbContext dbContext)
    : IDailyReportRepository
{
    public Task<DailyReport?> GetByIdAsync(
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DailyReports
            .Include(report => report.Activities)
            .SingleOrDefaultAsync(
                report => report.Id == reportId,
                cancellationToken);
    }

    public Task<bool> ExistsForProjectDateAsync(
        Guid projectId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DailyReports
            .AnyAsync(
                report =>
                    report.ProjectId == projectId &&
                    report.ReportDate == reportDate,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DailyReport>> ListForProjectAsync(
        Guid projectId,
        DailyReportStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.DailyReports
            .AsNoTracking()
            .Where(report => report.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                report => report.Status == status.Value);
        }

        return await query
            .OrderByDescending(report => report.ReportDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        DailyReportStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.DailyReports
            .AsNoTracking()
            .Where(report => report.ProjectId == projectId);

        if (status is not null)
        {
            query = query.Where(
                report => report.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        DailyReport report,
        CancellationToken cancellationToken = default)
    {
        await dbContext.DailyReports.AddAsync(
            report,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
