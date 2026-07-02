using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByActor;

/// <summary>Handles <see cref="GetActivityByActorQuery"/>.</summary>
public sealed class GetActivityByActorQueryHandler(IActivityLogRepository repository)
    : IRequestHandler<GetActivityByActorQuery, Result<IReadOnlyList<ActivityLogDto>>>
{
    /// <summary>Returns paged activity log entries performed by the specified actor.</summary>
    public async Task<Result<IReadOnlyList<ActivityLogDto>>> Handle(
        GetActivityByActorQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await repository.GetByActorAsync(
            request.ActorId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = logs.Select(l => new ActivityLogDto(
            l.Id,
            l.ActorId,
            l.Action.ToString(),
            l.EntityType,
            l.EntityId,
            l.EntityName,
            l.ProjectId,
            l.Metadata,
            l.OccurredAt)).ToList();

        return Result<IReadOnlyList<ActivityLogDto>>.Success(dtos);
    }
}
