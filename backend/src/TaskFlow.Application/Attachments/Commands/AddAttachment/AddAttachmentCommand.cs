using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Attachments.Commands.AddAttachment;

/// <summary>Records a new file attachment on a task (storage already handled by the caller).</summary>
public sealed record AddAttachmentCommand(
    Guid TaskId,
    Guid UploadedBy,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StorageUrl) : IRequest<Result<Guid>>;
