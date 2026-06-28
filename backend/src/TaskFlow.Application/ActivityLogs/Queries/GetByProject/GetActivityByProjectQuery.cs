using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetByProject;

/// <summary>Query to retrieve activity log entries scoped to a specific project.</summary>
/// <param name="ProjectId">The project identifier to filter by.</param>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of results per page.</param>
public sealed record GetActivityByProjectQuery(
    Guid ProjectId,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<ActivityLogDto>>>;
