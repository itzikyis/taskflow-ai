using MediatR;
using TaskFlow.Application.Teams.Queries.GetAllTeams;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Queries.GetTeamById;

/// <summary>Query to retrieve a single team by its identifier.</summary>
public sealed record GetTeamByIdQuery(Guid TeamId) : IRequest<Result<TeamDto>>;
