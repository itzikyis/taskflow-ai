using MediatR;
using TaskFlow.Application.AuditTrail.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Queries.GetByActor;

/// <summary>
/// Query that returns a paged list of audit entries for a specific actor, ordered newest-first.
/// </summary>
/// <param name="ActorId">The identifier of the actor (user).</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of entries per page.</param>
public sealed record GetAuditByActorQuery(
    Guid ActorId,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<AuditEntryDto>>>;
