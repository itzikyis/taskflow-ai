using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Teams.Commands.UpdateMemberRole;

/// <summary>Command to update the role of a team member.</summary>
public sealed record UpdateMemberRoleCommand(Guid TeamId, Guid UserId, TeamRole Role)
    : IRequest<Result>;
