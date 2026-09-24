using Domain.Approvals;
using Domain.Projects;
using Domain.PurchaseRequests;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PurchaseRequestConfiguration
    : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(
        EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.RequestNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(request => request.RequiredByDate)
            .HasColumnType("date");

        builder.Property(request => request.Purpose)
            .HasMaxLength(2000);

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(request => new
        {
            request.ProjectId,
            request.RequestNumber
        }).IsUnique();

        builder.HasIndex(request => request.RequestedByUserId);

        builder.HasIndex(request => request.ApprovalRequestId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(request => request.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(request => request.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApprovalRequest>()
            .WithMany()
            .HasForeignKey(request => request.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(request => request.Items)
            .WithOne()
            .HasForeignKey(item => item.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(request => request.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PurchaseRequestItemConfiguration
    : IEntityTypeConfiguration<PurchaseRequestItem>
{
    public void Configure(
        EntityTypeBuilder<PurchaseRequestItem> builder)
    {
        builder.ToTable("PurchaseRequestItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 3);

        builder.Property(item => item.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(item => item.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(item => item.PurchaseRequestId);
    }
}
