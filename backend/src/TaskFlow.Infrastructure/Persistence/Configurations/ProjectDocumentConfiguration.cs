using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ProjectDocument"/>.</summary>
internal sealed class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("project_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(d => d.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(d => d.Body).HasColumnName("body").IsRequired();
        builder.Property(d => d.AuthorId).HasColumnName("author_id").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Ignore(d => d.DomainEvents);

        builder.HasIndex(d => d.ProjectId).HasDatabaseName("ix_project_documents_project_id");
    }
}
