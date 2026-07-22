using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="CustomField"/>.</summary>
internal sealed class CustomFieldConfiguration : IEntityTypeConfiguration<CustomField>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CustomField> builder)
    {
        builder.ToTable("custom_fields");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(f => f.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(f => f.FieldType).HasColumnName("field_type").HasMaxLength(20).IsRequired();
        builder.Property(f => f.OptionsJson).HasColumnName("options_json").HasMaxLength(2000).IsRequired();
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Ignore(f => f.DomainEvents);

        builder.HasIndex(f => f.ProjectId).HasDatabaseName("ix_custom_fields_project_id");
    }
}
