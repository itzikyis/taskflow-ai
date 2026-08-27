using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Queries.GetTeamWorkload;

/// <summary>Query to retrieve workload data for all team members on a project.</summary>
/// <param name="ProjectId">The project to scope the workload query to.</param>
/// <param name="CapacityHoursPerWeek">Weekly capacity hours used to compute utilisation. Defaults to 40.</param>
public sealed record GetTeamWorkloadQuery(Guid ProjectId, double CapacityHoursPerWeek = 40) : IRequest<Result<TeamWorkloadDto>>;
