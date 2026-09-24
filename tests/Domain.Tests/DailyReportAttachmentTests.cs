using Domain.DailyReports;
using Xunit;

namespace Domain.Tests;

public sealed class DailyReportAttachmentTests
{
    [Fact]
    public void Draft_report_can_add_attachment()
    {
        var report = CreateReport();
        var attachmentId = Guid.NewGuid();

        var attachment = report.AddAttachment(
            attachmentId,
            $"daily-reports/{report.Id:N}/{attachmentId:N}.jpg",
            "site.jpg",
            "image/jpeg",
            1024,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            " BS-1 progress ");

        Assert.Single(report.Attachments);
        Assert.Equal("BS-1 progress", attachment.Caption);
        Assert.Equal("site.jpg", attachment.FileName);
    }

    [Fact]
    public void Submitted_report_rejects_attachment_changes()
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
            report.AddAttachment(
                Guid.NewGuid(),
                "daily-reports/file.pdf",
                "report.pdf",
                "application/pdf",
                100,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null));
    }

    [Fact]
    public void Attachment_requires_positive_size()
    {
        var report = CreateReport();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            report.AddAttachment(
                Guid.NewGuid(),
                "daily-reports/file.pdf",
                "report.pdf",
                "application/pdf",
                0,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null));
    }

    private static DailyReport CreateReport() =>
        DailyReport.Create(
            Guid.NewGuid(),
            new DateOnly(2026, 9, 24),
            Guid.NewGuid(),
            null,
            null);
}
