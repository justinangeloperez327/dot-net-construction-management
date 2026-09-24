using Domain.DailyReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DailyReportManpowerEntryConfiguration
    : IEntityTypeConfiguration<DailyReportManpowerEntry>
{
    public void Configure(
        EntityTypeBuilder<DailyReportManpowerEntry> builder)
    {
        builder.ToTable("DailyReportManpower");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Trade)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entry => entry.Contractor)
            .HasMaxLength(200);

        builder.Property(entry => entry.ManHours)
            .HasPrecision(10, 2);

        builder.Property(entry => entry.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(entry => entry.DailyReportId);
    }
}

public sealed class DailyReportEquipmentEntryConfiguration
    : IEntityTypeConfiguration<DailyReportEquipmentEntry>
{
    public void Configure(
        EntityTypeBuilder<DailyReportEquipmentEntry> builder)
    {
        builder.ToTable("DailyReportEquipment");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Equipment)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entry => entry.Identifier)
            .HasMaxLength(100);

        builder.Property(entry => entry.HoursUsed)
            .HasPrecision(10, 2);

        builder.Property(entry => entry.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(entry => entry.DailyReportId);
    }
}

public sealed class DailyReportSiteIssueConfiguration
    : IEntityTypeConfiguration<DailyReportSiteIssue>
{
    public void Configure(
        EntityTypeBuilder<DailyReportSiteIssue> builder)
    {
        builder.ToTable("DailyReportSiteIssues");

        builder.HasKey(issue => issue.Id);

        builder.Property(issue => issue.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(issue => issue.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(issue => issue.ActionTaken)
            .HasMaxLength(2000);

        builder.Property(issue => issue.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(issue => issue.DailyReportId);
    }
}
