using Domain.Documents;
using Domain.Projects;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration
    : IEntityTypeConfiguration<Document>
{
    public void Configure(
        EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.DocumentNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(document => document.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(document => document.Category)
            .HasMaxLength(100);

        builder.Property(document => document.Discipline)
            .HasMaxLength(100);

        builder.Property(document => document.Originator)
            .HasMaxLength(200);

        builder.Property(document => document.Description)
            .HasMaxLength(2000);

        builder.Property(document => document.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(document => new
        {
            document.ProjectId,
            document.DocumentNumber
        }).IsUnique();

        builder.HasIndex(document => document.CreatedByUserId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(document => document.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(document => document.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
