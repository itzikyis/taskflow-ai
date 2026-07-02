using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Commands.RecordAudit;

/// <summary>
/// Command to record a field-level audit entry for a domain entity mutation.
/// </summary>
/// <param name="ActorId">The user who performed the action.</param>
/// <param name="EntityType">The type name of the affected entity (e.g. "Task").</param>
/// <param name="EntityId">The identifier of the affected entity.</param>
/// <param name="Action">The action performed: "Created", "Updated", or "Deleted".</param>
/// <param name="Changes">
/// Optional JSON document describing field-level before/after values,
/// e.g. <c>{ "title": { "from": "old", "to": "new" } }</c>.
/// </param>
public sealed record RecordAuditCommand(
    Guid ActorId,
    string EntityType,
    Guid EntityId,
    string Action,
    string? Changes = null) : IRequest<Result<Guid>>;
