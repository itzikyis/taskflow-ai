using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a new project is created.</summary>
public sealed record ProjectCreatedEvent(
    Guid ProjectId,
    string Name,
    Guid OwnerId) : IDomainEvent;
