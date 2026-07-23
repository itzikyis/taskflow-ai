using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="CustomFieldValue"/>.</summary>
internal sealed class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.ToTable("custom_field_values");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.TaskId).HasColumnName("task_id").IsRequired();
        builder.Property(v => v.CustomFieldId).HasColumnName("custom_field_id").IsRequired();
        builder.Property(v => v.Value).HasColumnName("value").HasMaxLength(1000).IsRequired();

        builder.HasIndex(v => new { v.TaskId, v.CustomFieldId })
            .IsUnique()
            .HasDatabaseName("uq_custom_field_values_task_field");

        builder.HasIndex(v => v.TaskId).HasDatabaseName("ix_custom_field_values_task_id");
    }
}
