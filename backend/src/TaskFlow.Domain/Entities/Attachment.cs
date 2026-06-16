using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

/// <summary>Aggregate root representing a file attached to a task.</summary>
public sealed class Attachment : AggregateRoot
{
    private const long MaxFileSizeBytes = 100L * 1024 * 1024; // 100 MB

    private Attachment() { }

    private Attachment(Guid id, Guid taskId, Guid uploadedBy, string fileName, string contentType, long fileSizeBytes, string storageUrl)
    {
        Id = id;
        TaskId = taskId;
        UploadedBy = uploadedBy;
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        StorageUrl = storageUrl;
        UploadedAt = DateTime.UtcNow;
        RaiseDomainEvent(new AttachmentUploadedEvent(id, taskId, uploadedBy));
    }

    /// <summary>Gets the task this attachment belongs to.</summary>
    public Guid TaskId { get; private init; }

    /// <summary>Gets the id of the user who uploaded the file.</summary>
    public Guid UploadedBy { get; private init; }

    /// <summary>Gets the original file name.</summary>
    public string FileName { get; private init; } = string.Empty;

    /// <summary>Gets the MIME content type.</summary>
    public string ContentType { get; private init; } = string.Empty;

    /// <summary>Gets the file size in bytes.</summary>
    public long FileSizeBytes { get; private init; }

    /// <summary>Gets the storage URL where the file can be retrieved.</summary>
    public string StorageUrl { get; private init; } = string.Empty;

    /// <summary>Gets the upload timestamp (UTC).</summary>
    public DateTime UploadedAt { get; private init; }

    /// <summary>Records a new file attachment.</summary>
    public static Result<Attachment> Create(
        Guid taskId,
        Guid uploadedBy,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storageUrl)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return Result<Attachment>.Failure(AttachmentErrors.FileNameRequired);
        if (fileName.Length > 255) return Result<Attachment>.Failure(AttachmentErrors.FileNameTooLong);
        if (string.IsNullOrWhiteSpace(contentType)) return Result<Attachment>.Failure(AttachmentErrors.ContentTypeRequired);
        if (string.IsNullOrWhiteSpace(storageUrl)) return Result<Attachment>.Failure(AttachmentErrors.StorageUrlRequired);
        if (fileSizeBytes > MaxFileSizeBytes) return Result<Attachment>.Failure(AttachmentErrors.FileTooLarge);

        return Result<Attachment>.Success(
            new Attachment(Guid.NewGuid(), taskId, uploadedBy, fileName.Trim(), contentType.Trim(), fileSizeBytes, storageUrl.Trim()));
    }
}
