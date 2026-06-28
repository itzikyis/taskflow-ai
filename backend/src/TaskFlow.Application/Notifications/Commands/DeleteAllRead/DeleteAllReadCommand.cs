using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.DeleteAllRead;

/// <summary>Command to delete all read notifications for a user.</summary>
/// <param name="UserId">The user whose read notifications should be removed.</param>
public sealed record DeleteAllReadCommand(Guid UserId) : IRequest<Result>;
