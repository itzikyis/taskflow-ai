using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="User"/> aggregate.</summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Email)
               .HasColumnName("email")
               .HasMaxLength(254)
               .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.DisplayName)
               .HasColumnName("display_name")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(u => u.PasswordHash)
               .HasColumnName("password_hash")
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        // Domain events — not persisted
        builder.Ignore(u => u.DomainEvents);
    }
}
