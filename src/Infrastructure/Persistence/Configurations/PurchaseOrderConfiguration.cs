using Domain.Approvals;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;
using Domain.Suppliers;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration
    : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(
        EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.PurchaseOrderNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(order => order.PurchaseOrderNumber)
            .IsUnique();

        builder.Property(order => order.OrderDate)
            .HasColumnType("date");

        builder.Property(order => order.ExpectedDeliveryDate)
            .HasColumnType("date");

        builder.Property(order => order.Currency)
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(order => order.DeliveryAddress)
            .HasMaxLength(1000);

        builder.Property(order => order.DeliveryTerms)
            .HasMaxLength(1000);

        builder.Property(order => order.PaymentTerms)
            .HasMaxLength(1000);

        builder.Property(order => order.Notes)
            .HasMaxLength(2000);

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(order => order.ProjectId);
        builder.HasIndex(order => order.PurchaseRequestId);
        builder.HasIndex(order => order.SupplierId);
        builder.HasIndex(order => order.CreatedByUserId);
        builder.HasIndex(order => order.ApprovalRequestId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(order => order.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PurchaseRequest>()
            .WithMany()
            .HasForeignKey(order => order.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(order => order.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(order => order.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApprovalRequest>()
            .WithMany()
            .HasForeignKey(order => order.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PurchaseOrderItemConfiguration
    : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(
        EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.Quantity)
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

        builder.HasIndex(item => item.PurchaseOrderId);
        builder.HasIndex(item => item.PurchaseRequestItemId);

        builder.HasOne<PurchaseRequestItem>()
            .WithMany()
            .HasForeignKey(item => item.PurchaseRequestItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
