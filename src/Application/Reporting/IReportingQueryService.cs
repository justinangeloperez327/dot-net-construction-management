namespace Application.Reporting;

public interface IReportingQueryService
{
    Task<IReadOnlyList<ReportProjectOption>> ListProjectsAsync(
        CancellationToken cancellationToken = default);

    Task<ReportResult<PortfolioReportRow>> GetPortfolioAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default);

    Task<ReportResult<DailySiteReportRow>> GetDailySiteAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default);

    Task<ReportResult<ProcurementReportRow>> GetProcurementAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default);

    Task<ReportResult<CommercialReportRow>> GetCommercialAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default);
}
