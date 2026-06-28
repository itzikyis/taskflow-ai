using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.DeleteTeam;

/// <summary>Command to delete a team by its identifier.</summary>
public sealed record DeleteTeamCommand(Guid TeamId) : IRequest<Result>;
