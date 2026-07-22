using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.AI.Queries.GetDashboardInsights;

/// <summary>Returns an AI-narrated insight summary for the given project's dashboard.</summary>
public sealed record GetDashboardInsightsQuery(Guid ProjectId)
    : IRequest<Result<DashboardInsightsDto>>;
