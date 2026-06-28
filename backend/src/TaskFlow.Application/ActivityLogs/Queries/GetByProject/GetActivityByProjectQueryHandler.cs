using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByProject;

/// <summary>Handles <see cref="GetActivityByProjectQuery"/>.</summary>
public sealed class GetActivityByProjectQueryHandler(IActivityLogRepository repository)
    : IRequestHandler<GetActivityByProjectQuery, Result<IReadOnlyList<ActivityLogDto>>>
{
    /// <summary>Returns paged activity log entries for the specified project.</summary>
    public async Task<Result<IReadOnlyList<ActivityLogDto>>> Handle(
        GetActivityByProjectQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await repository.GetByProjectAsync(
            request.ProjectId,
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
