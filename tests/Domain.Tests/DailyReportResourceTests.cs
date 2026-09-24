using Domain.DailyReports;
using Xunit;

namespace Domain.Tests;

public sealed class DailyReportResourceTests
{
    [Fact]
    public void Manpower_requires_positive_headcount()
    {
        var report = CreateReport();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            report.AddManpower(
                "Carpenter",
                "Main Contractor",
                0,
                null,
                null));
    }

    [Fact]
    public void Equipment_requires_positive_quantity()
    {
        var report = CreateReport();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            report.AddEquipment(
                "Crane",
                "CR-01",
                0,
                null,
                null));
    }

    [Fact]
    public void Site_issue_requires_title_and_description()
    {
        var report = CreateReport();

        Assert.Throws<ArgumentException>(() =>
            report.AddSiteIssue(
                " ",
                "Blocked access",
                null,
                SiteIssueStatus.Open));
    }

    [Fact]
    public void Submitted_report_locks_resources_and_issues()
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
            report.AddManpower(
                "Electrician",
                null,
                2,
                16,
                null));

        Assert.Throws<InvalidOperationException>(() =>
            report.AddEquipment(
                "Generator",
                null,
                1,
                8,
                null));

        Assert.Throws<InvalidOperationException>(() =>
            report.AddSiteIssue(
                "Access",
                "Access blocked.",
                null,
                SiteIssueStatus.Open));
    }

    private static DailyReport CreateReport() =>
        DailyReport.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 24),
            Guid.NewGuid(),
            null,
            null);
}
