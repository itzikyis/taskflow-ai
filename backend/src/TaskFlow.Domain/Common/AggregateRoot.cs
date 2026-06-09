namespace TaskFlow.Domain.Common;

/// <summary>Base class for all aggregate roots. Tracks domain events.</summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Gets the unique identifier of this aggregate.</summary>
    public Guid Id { get; protected init; } = Guid.NewGuid();

    /// <summary>Gets domain events raised by this aggregate since it was last cleared.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Raises a domain event and appends it to the internal list.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>Clears all domain events (call after dispatching).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
