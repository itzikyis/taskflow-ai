using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.ActivityLogs.Commands.LogActivity;

/// <summary>
/// Internal command to record a significant system mutation as an activity log entry.
/// </summary>
/// <param name="ActorId">The user who performed the action.</param>
/// <param name="Action">The action that was performed.</param>
/// <param name="EntityType">The type of entity affected (e.g. "Task", "Comment").</param>
/// <param name="EntityId">The identifier of the affected entity.</param>
/// <param name="EntityName">Optional human-readable name snapshot of the entity.</param>
/// <param name="ProjectId">Optional project scope.</param>
/// <param name="Metadata">Optional JSON metadata for extra context.</param>
public sealed record LogActivityCommand(
    Guid ActorId,
    ActivityAction Action,
    string EntityType,
    Guid EntityId,
    string? EntityName = null,
    Guid? ProjectId = null,
    string? Metadata = null) : IRequest<Result<Guid>>;
