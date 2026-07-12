using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="TaskDependency"/> aggregate.</summary>
internal sealed class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    public void Configure(EntityTypeBuilder<TaskDependency> builder)
    {
        builder.ToTable("task_dependencies");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TaskId).HasColumnName("task_id").IsRequired();
        builder.Property(d => d.BlockedByTaskId).HasColumnName("blocked_by_task_id").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(d => d.TaskId);
        builder.HasIndex(d => d.BlockedByTaskId);
        builder.HasIndex(d => new { d.TaskId, d.BlockedByTaskId }).IsUnique();
        builder.Ignore(d => d.DomainEvents);
    }
}
