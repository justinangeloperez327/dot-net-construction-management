using Domain.Documents;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DocumentRevisionConfiguration
    : IEntityTypeConfiguration<DocumentRevision>
{
    public void Configure(
        EntityTypeBuilder<DocumentRevision> builder)
    {
        builder.ToTable("DocumentRevisions");

        builder.HasKey(revision => revision.Id);

        builder.Property(revision => revision.RevisionCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(revision => revision.ChangeSummary)
            .HasMaxLength(1000);

        builder.Property(revision => revision.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(revision => revision.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(revision => revision.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(revision => revision.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(revision => revision.ReviewComments)
            .HasMaxLength(2000);

        builder.HasIndex(revision => new
        {
            revision.DocumentId,
            revision.RevisionCode
        }).IsUnique();

        builder.HasIndex(revision => revision.StorageKey)
            .IsUnique();

        builder.HasIndex(revision => revision.CreatedByUserId);
        builder.HasIndex(revision => revision.SubmittedByUserId);
        builder.HasIndex(revision => revision.ReviewedByUserId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(revision => revision.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(revision => revision.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(revision => revision.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
