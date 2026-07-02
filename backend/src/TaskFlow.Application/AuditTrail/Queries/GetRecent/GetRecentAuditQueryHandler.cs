using MediatR;
using TaskFlow.Application.AuditTrail.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AuditTrail.Queries.GetRecent;

/// <summary>Handles <see cref="GetRecentAuditQuery"/>.</summary>
internal sealed class GetRecentAuditQueryHandler(IAuditRepository auditRepository)
    : IRequestHandler<GetRecentAuditQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(
        GetRecentAuditQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await auditRepository.GetRecentAsync(
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
