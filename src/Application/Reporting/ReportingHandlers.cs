namespace Application.Reporting;

public sealed class GetReportingProjectsHandler(
    IReportingQueryService reporting)
{
    public Task<IReadOnlyList<ReportProjectOption>> HandleAsync(
        CancellationToken cancellationToken = default) =>
        reporting.ListProjectsAsync(cancellationToken);
}

public sealed class GetPortfolioReportHandler(
    IReportingQueryService reporting)
{
    public Task<ReportResult<PortfolioReportRow>> HandleAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default) =>
        reporting.GetPortfolioAsync(
            filter,
            limit,
            cancellationToken);
}

public sealed class GetDailySiteReportHandler(
    IReportingQueryService reporting)
{
    public Task<ReportResult<DailySiteReportRow>> HandleAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default) =>
        reporting.GetDailySiteAsync(
            filter,
            limit,
            cancellationToken);
}

public sealed class GetProcurementReportHandler(
    IReportingQueryService reporting)
{
    public Task<ReportResult<ProcurementReportRow>> HandleAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default) =>
        reporting.GetProcurementAsync(
            filter,
            limit,
            cancellationToken);
}

public sealed class GetCommercialReportHandler(
    IReportingQueryService reporting)
{
    public Task<ReportResult<CommercialReportRow>> HandleAsync(
        ReportingFilter filter,
        int? limit = 200,
        CancellationToken cancellationToken = default) =>
        reporting.GetCommercialAsync(
            filter,
            limit,
            cancellationToken);
}
