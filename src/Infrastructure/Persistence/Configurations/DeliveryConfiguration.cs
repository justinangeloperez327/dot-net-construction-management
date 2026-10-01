using Domain.Deliveries;
using Domain.Projects;
using Domain.PurchaseOrders;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DeliveryConfiguration
    : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");

        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.DeliveryNoteNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(delivery => delivery.DeliveryDate)
            .HasColumnType("date");

        builder.Property(delivery => delivery.VehicleReference)
            .HasMaxLength(100);

        builder.Property(delivery => delivery.Remarks)
            .HasMaxLength(2000);

        builder.Property(delivery => delivery.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(delivery => new
        {
            delivery.PurchaseOrderId,
            delivery.DeliveryNoteNumber
        }).IsUnique();

        builder.HasIndex(delivery => new
        {
            delivery.PurchaseOrderId,
            delivery.Status,
            delivery.DeliveryDate
        });

        builder.HasIndex(delivery => delivery.ProjectId);

        builder.HasIndex(delivery => new
        {
            delivery.Status,
            delivery.ProjectId
        });
        builder.HasIndex(delivery => delivery.CreatedByUserId);
        builder.HasIndex(delivery => delivery.ReceivedByUserId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(delivery => delivery.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(delivery => delivery.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(delivery => delivery.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(delivery => delivery.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(delivery => delivery.Items)
            .WithOne()
            .HasForeignKey(item => item.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(delivery => delivery.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class DeliveryItemConfiguration
    : IEntityTypeConfiguration<DeliveryItem>
{
    public void Configure(EntityTypeBuilder<DeliveryItem> builder)
    {
        builder.ToTable("DeliveryItems");

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

        builder.HasIndex(item => item.DeliveryId);

        builder.HasIndex(item => new
        {
            item.DeliveryId,
            item.PurchaseOrderItemId
        }).IsUnique();

        builder.HasIndex(item => item.PurchaseOrderItemId);

        builder.HasOne<PurchaseOrderItem>()
            .WithMany()
            .HasForeignKey(item => item.PurchaseOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
