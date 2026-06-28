using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetRecent;

/// <summary>Handles <see cref="GetRecentActivityQuery"/>.</summary>
public sealed class GetRecentActivityQueryHandler(IActivityLogRepository repository)
    : IRequestHandler<GetRecentActivityQuery, Result<IReadOnlyList<ActivityLogDto>>>
{
    /// <summary>Returns the most recent paged activity log entries.</summary>
    public async Task<Result<IReadOnlyList<ActivityLogDto>>> Handle(
        GetRecentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await repository.GetRecentAsync(
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
