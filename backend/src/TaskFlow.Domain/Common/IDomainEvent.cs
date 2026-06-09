using MediatR;

namespace TaskFlow.Domain.Common;

/// <summary>Marker interface for all domain events.</summary>
public interface IDomainEvent : INotification { }
