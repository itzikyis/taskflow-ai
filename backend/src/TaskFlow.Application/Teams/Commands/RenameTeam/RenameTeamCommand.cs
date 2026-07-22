using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.RenameTeam;

/// <summary>Command to rename an existing team.</summary>
public sealed record RenameTeamCommand(Guid TeamId, string Name)
    : IRequest<Result>;
