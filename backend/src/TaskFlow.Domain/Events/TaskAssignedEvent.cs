using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a task is assigned to a user.</summary>
public sealed record TaskAssignedEvent(
    Guid TaskId,
    Guid AssignedToUserId) : IDomainEvent;
