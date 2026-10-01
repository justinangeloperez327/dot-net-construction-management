using Infrastructure.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.ActorEmail)
            .HasMaxLength(320);

        builder.Property(audit => audit.Action)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(audit => audit.EntityType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(audit => audit.EntityId)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(audit => audit.ChangesJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasIndex(audit => audit.OccurredAt);

        builder.HasIndex(audit => new
        {
            audit.ProjectId,
            audit.OccurredAt
        });

        builder.HasIndex(audit => new
        {
            audit.ActorUserId,
            audit.OccurredAt
        });

        builder.HasIndex(audit => new
        {
            audit.EntityType,
            audit.EntityId
        });
    }
}
