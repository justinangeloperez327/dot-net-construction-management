using Domain.Approvals;
using Domain.Projects;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ApprovalRequestConfiguration
    : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(
        EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("ApprovalRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.SubjectType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.Reference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(request => request.Description)
            .HasMaxLength(2000);

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.CancellationReason)
            .HasMaxLength(1000);

        builder.HasIndex(request => new
        {
            request.SubjectType,
            request.SubjectId
        });

        builder.HasIndex(request => request.ProjectId);

        builder.HasIndex(request => request.RequestedByUserId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(request => request.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(request => request.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(request => request.Steps)
            .WithOne()
            .HasForeignKey(step => step.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(request => request.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ApprovalStepConfiguration
    : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(
        EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");

        builder.HasKey(step => step.Id);

        builder.Property(step => step.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(step => step.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(step => step.Comments)
            .HasMaxLength(2000);

        builder.HasIndex(step => new
        {
            step.ApprovalRequestId,
            step.StepNumber
        }).IsUnique();

        builder.HasIndex(step => step.ApproverUserId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(step => step.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
