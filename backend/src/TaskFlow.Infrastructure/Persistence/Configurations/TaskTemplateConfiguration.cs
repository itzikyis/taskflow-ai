using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping configuration for <see cref="TaskTemplate"/>.</summary>
internal sealed class TaskTemplateConfiguration : IEntityTypeConfiguration<TaskTemplate>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TaskTemplate> builder)
    {
        builder.ToTable("task_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.DefaultTitle)
            .HasColumnName("default_title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.DefaultDescription)
            .HasColumnName("default_description")
            .HasMaxLength(2000);

        builder.Property(t => t.DefaultPriority)
            .HasColumnName("default_priority")
            .HasMaxLength(20);

        builder.Property(t => t.DefaultEstimatedHours)
            .HasColumnName("default_estimated_hours");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(t => t.ProjectId);

        builder.Ignore(t => t.DomainEvents);
    }
}
