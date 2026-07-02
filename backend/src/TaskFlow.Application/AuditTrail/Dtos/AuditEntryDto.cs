namespace TaskFlow.Application.AuditTrail.Dtos;

/// <summary>
/// Read-only projection of an <see cref="TaskFlow.Domain.Entities.AuditEntry"/> for API responses.
/// </summary>
/// <param name="Id">The unique identifier of the audit entry.</param>
/// <param name="ActorId">The identifier of the user who performed the action.</param>
/// <param name="EntityType">The type name of the affected entity.</param>
/// <param name="EntityId">The identifier of the affected entity.</param>
/// <param name="Action">The action performed: "Created", "Updated", or "Deleted".</param>
/// <param name="Changes">JSON document of field-level before/after values, or null.</param>
/// <param name="OccurredAt">UTC timestamp when the entry was recorded.</param>
public sealed record AuditEntryDto(
    Guid Id,
    Guid ActorId,
    string EntityType,
    Guid EntityId,
    string Action,
    string? Changes,
    DateTime OccurredAt);
