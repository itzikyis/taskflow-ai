using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

/// <summary>Append-only record of a significant mutation in the system.</summary>
public sealed class ActivityLog
{
    private ActivityLog() { }

    /// <summary>Gets the unique identifier of this log entry.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the identifier of the user who performed the action.</summary>
    public Guid ActorId { get; private set; }

    /// <summary>Gets the action that was performed.</summary>
    public ActivityAction Action { get; private set; }

    /// <summary>Gets the type of entity that was affected (e.g. "Task", "Comment", "Board").</summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>Gets the identifier of the affected entity.</summary>
    public Guid EntityId { get; private set; }

    /// <summary>Gets the human-readable name snapshot of the affected entity at the time of the action.</summary>
    public string? EntityName { get; private set; }

    /// <summary>Gets the optional project scope this action belongs to.</summary>
    public Guid? ProjectId { get; private set; }

    /// <summary>Gets optional JSON metadata providing extra context for the action.</summary>
    public string? Metadata { get; private set; }

    /// <summary>Gets the UTC timestamp when the action occurred.</summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// Creates a new <see cref="ActivityLog"/> entry.
    /// </summary>
    /// <param name="actorId">The user who performed the action.</param>
    /// <param name="action">The action that was performed.</param>
    /// <param name="entityType">The type of entity affected.</param>
    /// <param name="entityId">The identifier of the affected entity.</param>
    /// <param name="entityName">Optional human-readable name snapshot.</param>
    /// <param name="projectId">Optional project scope.</param>
    /// <param name="metadata">Optional JSON metadata.</param>
    /// <returns>A new <see cref="ActivityLog"/> instance.</returns>
    public static ActivityLog Create(
        Guid actorId,
        ActivityAction action,
        string entityType,
        Guid entityId,
        string? entityName = null,
        Guid? projectId = null,
        string? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            ProjectId = projectId,
            Metadata = metadata,
            OccurredAt = DateTime.UtcNow,
        };
    }
}
