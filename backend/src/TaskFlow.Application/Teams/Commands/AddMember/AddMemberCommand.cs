using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Teams.Commands.AddMember;

/// <summary>Command to add a user to a team.</summary>
public sealed record AddMemberCommand(Guid TeamId, Guid UserId, TeamRole Role)
    : IRequest<Result>;
