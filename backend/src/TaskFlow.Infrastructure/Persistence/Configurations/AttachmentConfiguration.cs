using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="Attachment"/> aggregate.</summary>
internal sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.TaskId).HasColumnName("task_id").IsRequired();
        builder.Property(a => a.UploadedBy).HasColumnName("uploaded_by").IsRequired();
        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
        builder.Property(a => a.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
        builder.Property(a => a.StorageUrl).HasColumnName("storage_url").HasMaxLength(2048).IsRequired();
        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        builder.Ignore(a => a.DomainEvents);
    }
}
