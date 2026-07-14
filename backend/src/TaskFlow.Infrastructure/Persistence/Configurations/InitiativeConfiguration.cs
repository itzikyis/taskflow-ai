using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="Initiative"/>.</summary>
internal sealed class InitiativeConfiguration : IEntityTypeConfiguration<Initiative>
{
    public void Configure(EntityTypeBuilder<Initiative> builder)
    {
        builder.ToTable("initiatives");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(i => i.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").IsRequired();
        builder.Property(i => i.Priority).HasColumnName("priority").IsRequired();
        builder.Property(i => i.Labels).HasColumnName("labels").HasMaxLength(500).IsRequired();
        builder.Property(i => i.StartDate).HasColumnName("start_date");
        builder.Property(i => i.TargetDate).HasColumnName("target_date");
        builder.Property(i => i.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Store project IDs as a pipe-delimited string.
        builder.Property(i => i.ProjectIdsRaw)
            .HasColumnName("project_ids")
            .HasMaxLength(4000)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Ignore(i => i.ProjectIds);
        builder.Ignore(i => i.DomainEvents);
    }
}
