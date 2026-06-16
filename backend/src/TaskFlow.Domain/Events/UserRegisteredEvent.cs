using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

/// <summary>Raised when a new user registers.</summary>
public sealed record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent;
