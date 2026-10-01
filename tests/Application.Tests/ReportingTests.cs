using System.Text;
using Application.Reporting;
using Domain.Commercial;
using Domain.DailyReports;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Xunit;

namespace Application.Tests;

public sealed class ReportingTests
{
    [Fact]
    public async Task Csv_export_requests_full_filtered_result()
    {
        var service = new FakeReportingQueryService();
        var handler = new ExportReportCsvHandler(service);

        await handler.HandleAsync(
            ReportKind.DailySite,
            new ReportingFilter(
                Guid.NewGuid(),
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 30)));

        Assert.Null(service.DailySiteLimit);
    }

    [Fact]
    public async Task Csv_export_escapes_commas_quotes_and_newlines()
    {
        var service = new FakeReportingQueryService
        {
            DailySiteRows =
            [
                new DailySiteReportRow(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "P-001",
                    "Tower, Phase \"A\"",
                    new DateOnly(2026, 10, 1),
                    DailyReportStatus.Approved,
                    "site@example.com",
                    "Clear\nWindy",
                    2,
                    12,
                    3,
                    1)
            ]
        };

        var handler = new ExportReportCsvHandler(service);

        var file = await handler.HandleAsync(
            ReportKind.DailySite,
            new ReportingFilter());

        Assert.Equal("daily-site-register.csv", file.FileName);
        Assert.True(file.Content.Length > 3);
        Assert.Equal(0xEF, file.Content[0]);
        Assert.Equal(0xBB, file.Content[1]);
        Assert.Equal(0xBF, file.Content[2]);

        var csv = Encoding.UTF8.GetString(file.Content);

        Assert.Contains(
            "\"Tower, Phase \"\"A\"\"\"",
            csv);

        Assert.Contains(
            "\"Clear\nWindy\"",
            csv);
    }

    [Fact]
    public void Report_result_marks_limited_screen_results_as_truncated()
    {
        var report = new ReportResult<int>(
            [1, 2],
            TotalCount: 10,
            Limit: 2);

        Assert.True(report.IsTruncated);
    }

    private sealed class FakeReportingQueryService
        : IReportingQueryService
    {
        public int? DailySiteLimit { get; private set; } = 200;

        public IReadOnlyList<DailySiteReportRow> DailySiteRows { get; set; } = [];

        public Task<IReadOnlyList<ReportProjectOption>> ListProjectsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ReportProjectOption>>([]);

        public Task<ReportResult<PortfolioReportRow>> GetPortfolioAsync(
            ReportingFilter filter,
            int? limit = 200,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                new ReportResult<PortfolioReportRow>(
                    [],
                    0,
                    limit));

        public Task<ReportResult<DailySiteReportRow>> GetDailySiteAsync(
            ReportingFilter filter,
            int? limit = 200,
            CancellationToken cancellationToken = default)
        {
            DailySiteLimit = limit;

            return Task.FromResult(
                new ReportResult<DailySiteReportRow>(
                    DailySiteRows,
                    DailySiteRows.Count,
                    limit));
        }

        public Task<ReportResult<ProcurementReportRow>> GetProcurementAsync(
            ReportingFilter filter,
            int? limit = 200,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                new ReportResult<ProcurementReportRow>(
                    [],
                    0,
                    limit));

        public Task<ReportResult<CommercialReportRow>> GetCommercialAsync(
            ReportingFilter filter,
            int? limit = 200,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                new ReportResult<CommercialReportRow>(
                    [],
                    0,
                    limit));
    }
}
