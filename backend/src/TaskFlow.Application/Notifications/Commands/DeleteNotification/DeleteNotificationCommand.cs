using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Commands.DeleteNotification;

/// <summary>Command to delete a single notification.</summary>
/// <param name="NotificationId">The notification to delete.</param>
/// <param name="RequesterId">The user making the request (must own the notification).</param>
public sealed record DeleteNotificationCommand(
    Guid NotificationId,
    Guid RequesterId) : IRequest<Result>;
