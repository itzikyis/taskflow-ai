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

        // Many-to-many project links stored in the junction table.
        builder.HasMany<InitiativeProject>("_projectLinks")
            .WithOne()
            .HasForeignKey(l => l.InitiativeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_projectLinks").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(i => i.ProjectIds);
        builder.Ignore(i => i.DomainEvents);
    }
}

/// <summary>EF Core configuration for <see cref="InitiativeProject"/>.</summary>
internal sealed class InitiativeProjectConfiguration : IEntityTypeConfiguration<InitiativeProject>
{
    public void Configure(EntityTypeBuilder<InitiativeProject> builder)
    {
        builder.ToTable("initiative_projects");
        builder.HasKey(l => new { l.InitiativeId, l.ProjectId });
        builder.Property(l => l.InitiativeId).HasColumnName("initiative_id");
        builder.Property(l => l.ProjectId).HasColumnName("project_id");
    }
}
