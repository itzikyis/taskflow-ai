namespace TaskFlow.Domain.Entities;

/// <summary>
/// Immutable record of a field-level change to a domain entity, used for compliance and forensic audit trails.
/// </summary>
public sealed class AuditEntry
{
    private AuditEntry() { }

    /// <summary>Gets the unique identifier for this audit entry.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the identifier of the user who performed the action.</summary>
    public Guid ActorId { get; private set; }

    /// <summary>Gets the type name of the affected entity (e.g. "Task").</summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>Gets the identifier of the affected entity.</summary>
    public Guid EntityId { get; private set; }

    /// <summary>Gets the action performed: "Created", "Updated", or "Deleted".</summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a JSON document describing field-level before/after values,
    /// e.g. <c>{ "title": { "from": "old", "to": "new" } }</c>. Null for Created/Deleted.
    /// </summary>
    public string? Changes { get; private set; }

    /// <summary>Gets the UTC timestamp when this audit entry was recorded.</summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// Creates a new <see cref="AuditEntry"/> instance.
    /// </summary>
    /// <param name="actorId">The user who performed the action.</param>
    /// <param name="entityType">The type name of the affected entity.</param>
    /// <param name="entityId">The identifier of the affected entity.</param>
    /// <param name="action">The action: "Created", "Updated", or "Deleted".</param>
    /// <param name="changes">Optional JSON diff of field-level changes.</param>
    /// <returns>A new, hydrated <see cref="AuditEntry"/>.</returns>
    public static AuditEntry Create(
        Guid actorId,
        string entityType,
        Guid entityId,
        string action,
        string? changes = null)
    {
        return new AuditEntry
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Changes = changes,
            OccurredAt = DateTime.UtcNow,
        };
    }
}
