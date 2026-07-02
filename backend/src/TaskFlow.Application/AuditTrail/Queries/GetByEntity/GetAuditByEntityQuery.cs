using MediatR;
using TaskFlow.Application.AuditTrail.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Queries.GetByEntity;

/// <summary>
/// Query that returns all audit entries for a specific entity, ordered newest-first.
/// </summary>
/// <param name="EntityType">The type name of the entity (e.g. "Task").</param>
/// <param name="EntityId">The identifier of the entity.</param>
public sealed record GetAuditByEntityQuery(
    string EntityType,
    Guid EntityId) : IRequest<Result<IReadOnlyList<AuditEntryDto>>>;
