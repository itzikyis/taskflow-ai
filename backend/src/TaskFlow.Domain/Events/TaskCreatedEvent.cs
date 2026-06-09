using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a new task is created.</summary>
public sealed record TaskCreatedEvent(
    Guid TaskId,
    string Title,
    Guid CreatedByUserId) : IDomainEvent;
