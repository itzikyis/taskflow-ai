using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.AuditTrail.Commands.RecordAudit;

/// <summary>Handles <see cref="RecordAuditCommand"/> by persisting an immutable <see cref="AuditEntry"/>.</summary>
public sealed class RecordAuditCommandHandler(IAuditRepository auditRepository)
    : IRequestHandler<RecordAuditCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        RecordAuditCommand request,
        CancellationToken cancellationToken)
    {
        var entry = AuditEntry.Create(
            request.ActorId,
            request.EntityType,
            request.EntityId,
            request.Action,
            request.Changes);

        await auditRepository.AddAsync(entry, cancellationToken);
        await auditRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entry.Id);
    }
}
