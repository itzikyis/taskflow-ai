using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a new board is created.</summary>
public sealed record BoardCreatedEvent(Guid BoardId, string Name, Guid ProjectId) : IDomainEvent;
