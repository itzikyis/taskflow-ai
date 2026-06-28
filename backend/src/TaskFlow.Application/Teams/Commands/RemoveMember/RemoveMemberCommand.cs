using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Teams.Commands.RemoveMember;

/// <summary>Command to remove a user from a team.</summary>
public sealed record RemoveMemberCommand(Guid TeamId, Guid UserId) : IRequest<Result>;
