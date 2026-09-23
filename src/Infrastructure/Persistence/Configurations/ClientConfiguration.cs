using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration
    : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(client => client.Id);

        builder.Property(client => client.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(client => client.Name);

        builder.Property(client => client.ContactPerson)
            .HasMaxLength(200);

        builder.Property(client => client.Email)
            .HasMaxLength(320);

        builder.Property(client => client.Phone)
            .HasMaxLength(50);

        builder.Property(client => client.Address)
            .HasMaxLength(1000);

        builder.Property(client => client.IsActive)
            .HasDefaultValue(true);
    }
}
