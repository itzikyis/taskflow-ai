using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Query to retrieve workload data for all team members on a project.</summary>
public sealed record GetTeamWorkloadQuery(Guid ProjectId) : IRequest<Result<TeamWorkloadDto>>;
