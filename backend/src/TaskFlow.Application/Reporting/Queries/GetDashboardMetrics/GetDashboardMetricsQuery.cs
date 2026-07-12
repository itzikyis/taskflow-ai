using MediatR;
using TaskFlow.Application.Reporting.Dtos;

namespace TaskFlow.Application.Reporting.Queries.GetDashboardMetrics;

/// <summary>Query returning aggregate project analytics for the dashboard.</summary>
public sealed record GetDashboardMetricsQuery : IRequest<DashboardMetricsDto>;
