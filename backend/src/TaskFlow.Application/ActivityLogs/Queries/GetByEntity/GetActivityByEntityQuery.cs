using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByEntity;

/// <summary>Query to retrieve activity log entries for a specific entity.</summary>
/// <param name="EntityType">The entity type to filter by (e.g. "Task").</param>
/// <param name="EntityId">The entity identifier to filter by.</param>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of results per page.</param>
public sealed record GetActivityByEntityQuery(
    string EntityType,
    Guid EntityId,
    int Page = 1,
    int PageSize = 30) : IRequest<Result<IReadOnlyList<ActivityLogDto>>>;
