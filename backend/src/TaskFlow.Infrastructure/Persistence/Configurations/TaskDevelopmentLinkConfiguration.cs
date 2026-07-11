using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="TaskDevelopmentLink"/> aggregate.</summary>
internal sealed class TaskDevelopmentLinkConfiguration : IEntityTypeConfiguration<TaskDevelopmentLink>
{
    public void Configure(EntityTypeBuilder<TaskDevelopmentLink> builder)
    {
        builder.ToTable("task_development_links");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.TaskId).HasColumnName("task_id").IsRequired();
        builder.Property(l => l.Repository).HasColumnName("repository").HasMaxLength(255).IsRequired();
        builder.Property(l => l.RefType).HasColumnName("ref_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(l => l.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        builder.Property(l => l.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.ExternalId).HasColumnName("external_id").HasMaxLength(255);
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(l => l.TaskId);
        builder.Ignore(l => l.DomainEvents);
    }
}
