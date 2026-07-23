using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Reporting.Queries.GetProjectAnalytics;

/// <summary>Query returning velocity and status breakdown analytics for a project.</summary>
public sealed record GetProjectAnalyticsQuery(Guid ProjectId) : IRequest<Result<ProjectAnalyticsDto>>;
