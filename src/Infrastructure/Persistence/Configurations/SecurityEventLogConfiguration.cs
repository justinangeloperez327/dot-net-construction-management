using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SecurityEventLogConfiguration
    : IEntityTypeConfiguration<SecurityEventLog>
{
    public void Configure(EntityTypeBuilder<SecurityEventLog> builder)
    {
        builder.ToTable("SecurityEvents");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.EventType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.Email)
            .HasMaxLength(320);

        builder.Property(item => item.RemoteIpAddress)
            .HasMaxLength(64);

        builder.Property(item => item.UserAgent)
            .HasMaxLength(512);

        builder.Property(item => item.RequestPath)
            .HasMaxLength(2048);

        builder.HasIndex(item => item.OccurredAt);
        builder.HasIndex(item => new { item.EventType, item.OccurredAt });
        builder.HasIndex(item => new { item.UserId, item.OccurredAt });
    }
}
