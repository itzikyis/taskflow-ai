namespace TaskFlow.Application.Attachments.Dtos;

/// <summary>DTO representing a file attachment.</summary>
public sealed record AttachmentDto(
    Guid Id,
    Guid TaskId,
    Guid UploadedBy,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StorageUrl,
    DateTime UploadedAt);
