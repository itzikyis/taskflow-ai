using MediatR;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.ActivityLogs.Commands.LogActivity;

/// <summary>Handles <see cref="LogActivityCommand"/> by persisting a new activity log entry.</summary>
public sealed class LogActivityCommandHandler(IActivityLogRepository repository)
    : IRequestHandler<LogActivityCommand, Result<Guid>>
{
    /// <summary>Creates an activity log entry and persists it.</summary>
    public async Task<Result<Guid>> Handle(LogActivityCommand request, CancellationToken cancellationToken)
    {
        var log = ActivityLog.Create(
            request.ActorId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.EntityName,
            request.ProjectId,
            request.Metadata);

        await repository.AddAsync(log, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(log.Id);
    }
}
