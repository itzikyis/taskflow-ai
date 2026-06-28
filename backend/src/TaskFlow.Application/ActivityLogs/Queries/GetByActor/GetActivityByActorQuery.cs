using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByActor;

/// <summary>Query to retrieve activity log entries performed by a specific actor.</summary>
/// <param name="ActorId">The actor identifier to filter by.</param>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of results per page.</param>
public sealed record GetActivityByActorQuery(
    Guid ActorId,
    int Page = 1,
    int PageSize = 30) : IRequest<Result<IReadOnlyList<ActivityLogDto>>>;
