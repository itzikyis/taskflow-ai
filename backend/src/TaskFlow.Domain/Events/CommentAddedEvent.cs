using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a comment is added to a task.</summary>
public sealed record CommentAddedEvent(Guid CommentId, Guid TaskId, Guid AuthorId) : IDomainEvent;
