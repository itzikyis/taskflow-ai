using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.MarkAllAsRead;

/// <summary>Command to mark all unread notifications for a user as read.</summary>
/// <param name="UserId">The user whose notifications should be marked read.</param>
public sealed record MarkAllAsReadCommand(Guid UserId) : IRequest<Result>;
