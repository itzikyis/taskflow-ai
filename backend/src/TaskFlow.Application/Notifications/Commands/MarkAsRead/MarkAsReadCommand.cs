using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.MarkAsRead;

/// <summary>Command to mark a single notification as read.</summary>
/// <param name="NotificationId">The notification to mark as read.</param>
/// <param name="RequesterId">The user making the request (must own the notification).</param>
public sealed record MarkAsReadCommand(
    Guid NotificationId,
    Guid RequesterId) : IRequest<Result>;
