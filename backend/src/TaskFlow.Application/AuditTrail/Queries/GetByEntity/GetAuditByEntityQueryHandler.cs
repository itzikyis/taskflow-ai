using MediatR;
using TaskFlow.Application.AuditTrail.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Queries.GetByEntity;

/// <summary>Handles <see cref="GetAuditByEntityQuery"/>.</summary>
internal sealed class GetAuditByEntityQueryHandler(IAuditRepository auditRepository)
    : IRequestHandler<GetAuditByEntityQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(
        GetAuditByEntityQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await auditRepository.GetByEntityAsync(
            request.EntityType,
            request.EntityId,
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
