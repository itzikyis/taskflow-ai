using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.CreateTeam;

/// <summary>Command to create a new team.</summary>
public sealed record CreateTeamCommand(string Name, string? Description)
    : IRequest<Result<Guid>>;
