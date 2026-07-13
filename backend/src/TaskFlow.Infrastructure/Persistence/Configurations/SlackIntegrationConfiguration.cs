using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="SlackIntegration"/> aggregate.</summary>
internal sealed class SlackIntegrationConfiguration : IEntityTypeConfiguration<SlackIntegration>
{
    public void Configure(EntityTypeBuilder<SlackIntegration> builder)
    {
        builder.ToTable("slack_integrations");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.WebhookUrl).HasColumnName("webhook_url").HasMaxLength(1024).IsRequired();
        builder.Property(s => s.Enabled).HasColumnName("enabled").IsRequired();
        builder.Property(s => s.ForwardCreated).HasColumnName("forward_created").IsRequired();
        builder.Property(s => s.ForwardStatusChanged).HasColumnName("forward_status_changed").IsRequired();
        builder.Property(s => s.ForwardComments).HasColumnName("forward_comments").IsRequired();
        builder.Property(s => s.ForwardOther).HasColumnName("forward_other").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(s => s.DomainEvents);
    }
}
