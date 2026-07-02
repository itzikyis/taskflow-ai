using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping configuration for <see cref="AuditEntry"/>.</summary>
internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    /// <summary>Configures the <see cref="AuditEntry"/> entity mapping.</summary>
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Changes)
            .HasColumnName("changes")
            .HasColumnType("text");

        builder.Property(a => a.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("ix_audit_entries_entity");

        builder.HasIndex(a => a.ActorId)
            .HasDatabaseName("ix_audit_entries_actor");

        builder.HasIndex(a => a.OccurredAt)
            .HasDatabaseName("ix_audit_entries_recent");
    }
}
