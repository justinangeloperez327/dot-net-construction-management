using Domain.DailyReports;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DailyReportAttachmentConfiguration
    : IEntityTypeConfiguration<DailyReportAttachment>
{
    public void Configure(
        EntityTypeBuilder<DailyReportAttachment> builder)
    {
        builder.ToTable("DailyReportAttachments");

        builder.HasKey(attachment => attachment.Id);

        builder.Property(attachment => attachment.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(attachment => attachment.StorageKey)
            .IsUnique();

        builder.Property(attachment => attachment.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(attachment => attachment.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(attachment => attachment.Caption)
            .HasMaxLength(500);

        builder.HasIndex(attachment => attachment.DailyReportId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(attachment => attachment.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(attachment => attachment.UploadedByUserId);
    }
}
