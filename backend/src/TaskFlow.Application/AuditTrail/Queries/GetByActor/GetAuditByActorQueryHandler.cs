using MediatR;
using TaskFlow.Application.AuditTrail.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Queries.GetByActor;

/// <summary>Handles <see cref="GetAuditByActorQuery"/>.</summary>
internal sealed class GetAuditByActorQueryHandler(IAuditRepository auditRepository)
    : IRequestHandler<GetAuditByActorQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(
        GetAuditByActorQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await auditRepository.GetByActorAsync(
            request.ActorId,
            request.Page,
            request.PageSize,
            cancellationToken);

        IReadOnlyList<AuditEntryDto> dtos = entries
            .Select(e => new AuditEntryDto(
                e.Id,
                e.ActorId,
                e.EntityType,
                e.EntityId,
                e.Action,
                e.Changes,
                e.OccurredAt))
            .ToList();

        return Result<IReadOnlyList<AuditEntryDto>>.Success(dtos);
    }
}
