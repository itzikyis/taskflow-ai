using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping configuration for <see cref="ActivityLog"/>.</summary>
internal sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    /// <summary>Configures the <see cref="ActivityLog"/> entity mapping.</summary>
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.ActorId)
            .HasColumnName("actor_id")
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(a => a.EntityName)
            .HasColumnName("entity_name")
            .HasMaxLength(200);

        builder.Property(a => a.ProjectId)
            .HasColumnName("project_id");

        builder.Property(a => a.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("text");

        builder.Property(a => a.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("ix_activity_logs_entity");

        builder.HasIndex(a => a.ProjectId)
            .HasDatabaseName("ix_activity_logs_project");

        builder.HasIndex(a => a.ActorId)
            .HasDatabaseName("ix_activity_logs_actor");

        builder.HasIndex(a => a.OccurredAt)
            .HasDatabaseName("ix_activity_logs_recent");
    }
}
