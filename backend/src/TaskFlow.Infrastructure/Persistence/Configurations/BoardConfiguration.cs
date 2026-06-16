using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="Board"/> aggregate.</summary>
internal sealed class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("boards");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(b => b.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(b => b.DomainEvents);

        builder.OwnsMany(b => b.Columns, col =>
        {
            col.ToTable("board_columns");
            col.WithOwner().HasForeignKey("board_id");
            col.HasKey(c => c.Id);
            col.Property(c => c.Id).HasColumnName("id");
            col.Property(c => c.BoardId).HasColumnName("board_id");
            col.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            col.Property(c => c.Order).HasColumnName("ord").IsRequired();
            col.Property(c => c.WipLimit).HasColumnName("wip_limit");
        });
    }
}
