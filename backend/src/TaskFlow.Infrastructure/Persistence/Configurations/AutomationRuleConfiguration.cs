using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="AutomationRule"/>.</summary>
internal sealed class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("automation_rules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(r => r.IsEnabled).HasColumnName("is_enabled").IsRequired();
        builder.Property(r => r.TriggerType).HasColumnName("trigger_type").IsRequired();
        builder.Property(r => r.TriggerValue).HasColumnName("trigger_value").HasMaxLength(100).IsRequired();
        builder.Property(r => r.ActionType).HasColumnName("action_type").IsRequired();
        builder.Property(r => r.ActionValue).HasColumnName("action_value").HasMaxLength(500).IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Ignore(r => r.DomainEvents);

        builder.HasIndex(r => r.ProjectId).HasDatabaseName("ix_automation_rules_project_id");
    }
}
