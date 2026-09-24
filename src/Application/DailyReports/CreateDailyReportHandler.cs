using Application.Common.Authentication;
using Application.Projects;
using Domain.DailyReports;
using Domain.Projects;

namespace Application.DailyReports;

public sealed record CreateDailyReportRequest(
    Guid ProjectId,
    DateOnly ReportDate,
    string? Weather,
    string? Remarks);

public sealed class CreateDailyReportHandler(
    IDailyReportRepository reports,
    IProjectRepository projects,
    ICurrentUser currentUser)
{
    public async Task<DailyReportActionResult> HandleAsync(
        CreateDailyReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return DailyReportActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return DailyReportActionResult.Failure(
                "Daily reports cannot be created for a closed project.");
        }

        if (await reports.ExistsForProjectDateAsync(
                request.ProjectId,
                request.ReportDate,
                cancellationToken))
        {
            return DailyReportActionResult.Failure(
                "A daily report already exists for this project and date.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DailyReportActionResult.Failure(
                "An authenticated user is required.");
        }

        var report = DailyReport.Create(
            request.ProjectId,
            request.ReportDate,
            userId,
            request.Weather,
            request.Remarks);

        await reports.AddAsync(report, cancellationToken);
        await reports.SaveChangesAsync(cancellationToken);

        return DailyReportActionResult.Success(report.Id);
    }
}
