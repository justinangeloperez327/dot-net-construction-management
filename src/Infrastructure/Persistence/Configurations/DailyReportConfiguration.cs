using Domain.DailyReports;
using Domain.Projects;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DailyReportConfiguration
    : IEntityTypeConfiguration<DailyReport>
{
    public void Configure(
        EntityTypeBuilder<DailyReport> builder)
    {
        builder.ToTable("DailyReports");

        builder.HasKey(report => report.Id);

        builder.HasIndex(report => new
        {
            report.ProjectId,
            report.ReportDate
        }).IsUnique();

        builder.Property(report => report.ReportDate)
            .HasColumnType("date");

        builder.Property(report => report.Weather)
            .HasMaxLength(200);

        builder.Property(report => report.Remarks)
            .HasMaxLength(2000);

        builder.Property(report => report.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(report => report.ReviewComments)
            .HasMaxLength(2000);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(report => report.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(report => report.PreparedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(report => report.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(report => report.Activities)
            .WithOne()
            .HasForeignKey(activity => activity.DailyReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(report => report.Activities)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class DailyReportActivityConfiguration
    : IEntityTypeConfiguration<DailyReportActivity>
{
    public void Configure(
        EntityTypeBuilder<DailyReportActivity> builder)
    {
        builder.ToTable("DailyReportActivities");

        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.WorkArea)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(activity => activity.Activity)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(activity => activity.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(activity => activity.ProgressPercent)
            .HasPrecision(5, 2);

        builder.Property(activity => activity.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(activity => activity.DailyReportId);
    }
}
