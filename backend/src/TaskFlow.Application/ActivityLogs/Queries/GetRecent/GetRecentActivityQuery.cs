using MediatR;
using TaskFlow.Application.ActivityLogs.Dtos;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.ActivityLogs.Queries.GetRecent;

/// <summary>Query to retrieve the most recent activity log entries across the system.</summary>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of results per page.</param>
public sealed record GetRecentActivityQuery(
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<ActivityLogDto>>>;
