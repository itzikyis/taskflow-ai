using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping configuration for <see cref="Goal"/> and its <see cref="KeyResult"/> children.</summary>
internal sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id");

        builder.Property(g => g.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(g => g.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(g => g.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasColumnName("description");

        builder.Property(g => g.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.ProgressPercent)
            .HasColumnName("progress_percent")
            .IsRequired();

        builder.Property(g => g.DueDate)
            .HasColumnName("due_date");

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Ignore(g => g.DomainEvents);

        builder.HasMany(g => g.KeyResults)
            .WithOne()
            .HasForeignKey(kr => kr.GoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(g => g.KeyResults).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>EF Core mapping configuration for <see cref="KeyResult"/>.</summary>
internal sealed class KeyResultConfiguration : IEntityTypeConfiguration<KeyResult>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<KeyResult> builder)
    {
        builder.ToTable("key_results");

        builder.HasKey(kr => kr.Id);

        builder.Property(kr => kr.Id)
            .HasColumnName("id");

        builder.Property(kr => kr.GoalId)
            .HasColumnName("goal_id")
            .IsRequired();

        builder.Property(kr => kr.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(kr => kr.TargetValue)
            .HasColumnName("target_value")
            .IsRequired();

        builder.Property(kr => kr.CurrentValue)
            .HasColumnName("current_value")
            .IsRequired();

        builder.Property(kr => kr.Unit)
            .HasColumnName("unit")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(kr => kr.LinkedTaskIds)
            .HasColumnName("linked_task_ids");

        builder.Ignore(kr => kr.ProgressPercent);
    }
}
