using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a file is attached to a task.</summary>
public sealed record AttachmentUploadedEvent(Guid AttachmentId, Guid TaskId, Guid UploadedBy) : IDomainEvent;
