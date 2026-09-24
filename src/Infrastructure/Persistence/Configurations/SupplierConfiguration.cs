using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration
    : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.SupplierCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(supplier => supplier.SupplierCode)
            .IsUnique();

        builder.Property(supplier => supplier.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(supplier => supplier.Name);

        builder.Property(supplier => supplier.Category)
            .HasMaxLength(150);

        builder.Property(supplier => supplier.ContactPerson)
            .HasMaxLength(200);

        builder.Property(supplier => supplier.Email)
            .HasMaxLength(320);

        builder.Property(supplier => supplier.Phone)
            .HasMaxLength(50);

        builder.Property(supplier => supplier.Address)
            .HasMaxLength(1000);

        builder.Property(supplier => supplier.RegistrationNumber)
            .HasMaxLength(100);

        builder.Property(supplier => supplier.TaxRegistrationNumber)
            .HasMaxLength(100);

        builder.Property(supplier => supplier.IsActive)
            .HasDefaultValue(true);
    }
}
