using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByEntity;

/// <summary>Handles <see cref="GetActivityByEntityQuery"/>.</summary>
public sealed class GetActivityByEntityQueryHandler(IActivityLogRepository repository)
    : IRequestHandler<GetActivityByEntityQuery, Result<IReadOnlyList<ActivityLogDto>>>
{
    /// <summary>Returns paged activity log entries for the specified entity.</summary>
    public async Task<Result<IReadOnlyList<ActivityLogDto>>> Handle(
        GetActivityByEntityQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await repository.GetByEntityAsync(
            request.EntityType,
            request.EntityId,
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
