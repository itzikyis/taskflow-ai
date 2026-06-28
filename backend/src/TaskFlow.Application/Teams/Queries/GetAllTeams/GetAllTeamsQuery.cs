using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Queries.GetAllTeams;

/// <summary>Query to retrieve all teams.</summary>
public sealed record GetAllTeamsQuery() : IRequest<Result<IReadOnlyList<TeamDto>>>;
