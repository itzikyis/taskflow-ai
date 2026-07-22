using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="CalendarSubscription"/> aggregate.</summary>
internal sealed class CalendarSubscriptionConfiguration : IEntityTypeConfiguration<CalendarSubscription>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CalendarSubscription> builder)
    {
        builder.ToTable("calendar_subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(s => s.ExternalUrl)
            .HasColumnName("external_url")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.LastSyncedAt)
            .HasColumnName("last_synced_at");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(s => s.ProjectId)
            .HasDatabaseName("ix_calendar_subscriptions_project_id");

        builder.Ignore(s => s.DomainEvents);
    }
}
