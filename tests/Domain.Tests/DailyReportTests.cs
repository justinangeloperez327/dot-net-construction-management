using Domain.DailyReports;
using Xunit;

namespace Domain.Tests;

public sealed class DailyReportTests
{
    [Fact]
    public void New_report_is_draft_and_normalizes_header()
    {
        var report = DailyReport.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 24),
            Guid.NewGuid(),
            " Clear ",
            " Normal progress ");

        Assert.Equal(DailyReportStatus.Draft, report.Status);
        Assert.Equal("Clear", report.Weather);
        Assert.Equal("Normal progress", report.Remarks);
    }

    [Fact]
    public void Submit_requires_at_least_one_activity()
    {
        var report = CreateReport();

        Assert.Throws<InvalidOperationException>(() =>
            report.Submit(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Submitted_report_cannot_be_edited()
    {
        var report = CreateReport();

        report.AddActivity(
            "BS-1",
            "AC ducting",
            DailyActivityStatus.InProgress,
            50,
            null);

        report.Submit(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            report.UpdateHeader("Cloudy", null));
    }

    [Fact]
    public void Rejected_report_becomes_editable_and_can_be_resubmitted()
    {
        var report = CreateReport();

        report.AddActivity(
            "BS-1",
            "AC ducting",
            DailyActivityStatus.InProgress,
            50,
            null);

        report.Submit(DateTimeOffset.UtcNow);
        report.Reject(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "Update progress.");

        report.UpdateHeader("Clear", "Corrected.");
        report.Submit(DateTimeOffset.UtcNow);

        Assert.Equal(DailyReportStatus.Submitted, report.Status);
        Assert.Null(report.ReviewComments);
    }

    [Fact]
    public void Reject_requires_comments()
    {
        var report = CreateReport();

        report.AddActivity(
            "BS-1",
            "AC ducting",
            DailyActivityStatus.InProgress,
            null,
            null);

        report.Submit(DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() =>
            report.Reject(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                " "));
    }

    private static DailyReport CreateReport() =>
        DailyReport.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 24),
            Guid.NewGuid(),
            null,
            null);
}
