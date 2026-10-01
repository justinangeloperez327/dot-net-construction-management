using Domain.Approvals;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PaymentApplicationConfiguration
    : IEntityTypeConfiguration<PaymentApplication>
{
    public void Configure(
        EntityTypeBuilder<PaymentApplication> builder)
    {
        builder.ToTable("PaymentApplications");

        builder.HasKey(application => application.Id);

        builder.Property(application => application.ApplicationNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(application => application.ApplicationDate)
            .HasColumnType("date");

        builder.Property(application => application.PeriodFrom)
            .HasColumnType("date");

        builder.Property(application => application.PeriodTo)
            .HasColumnType("date");

        builder.Property(application => application.Notes)
            .HasMaxLength(2000);

        builder.Property(application => application.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(application => new
        {
            application.PurchaseOrderId,
            application.ApplicationNumber
        }).IsUnique();

        builder.HasIndex(application => application.ProjectId);

        builder.HasIndex(application => new
        {
            application.Status,
            application.ProjectId
        });

        builder.HasIndex(application => new
        {
            application.ProjectId,
            application.ApplicationDate,
            application.ApplicationNumber
        });
        builder.HasIndex(application => application.CreatedByUserId);
        builder.HasIndex(application => application.ApprovalRequestId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(application => application.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(application => application.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(application => application.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApprovalRequest>()
            .WithMany()
            .HasForeignKey(application => application.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(application => application.Items)
            .WithOne()
            .HasForeignKey(item => item.PaymentApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(application => application.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PaymentApplicationItemConfiguration
    : IEntityTypeConfiguration<PaymentApplicationItem>
{
    public void Configure(
        EntityTypeBuilder<PaymentApplicationItem> builder)
    {
        builder.ToTable("PaymentApplicationItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.ClaimedQuantity)
            .HasPrecision(18, 3);

        builder.Property(item => item.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(item => item.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(item => item.TaxPercent)
            .HasPrecision(5, 2);

        builder.Property(item => item.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(item => item.PurchaseOrderItemId);

        builder.HasIndex(item => new
        {
            item.PaymentApplicationId,
            item.PurchaseOrderItemId
        }).IsUnique();

        builder.HasOne<PurchaseOrderItem>()
            .WithMany()
            .HasForeignKey(item => item.PurchaseOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
