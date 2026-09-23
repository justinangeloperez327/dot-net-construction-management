using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration
    : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.ProjectNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(project => project.ProjectNumber)
            .IsUnique();

        builder.Property(project => project.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Location)
            .HasMaxLength(250);

        builder.Property(project => project.StartDate)
            .HasColumnType("date");

        builder.Property(project => project.TargetCompletionDate)
            .HasColumnType("date");

        builder.Property(project => project.Description)
            .HasMaxLength(2000);

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }
}
