using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Notifications.Commands.CreateNotification;

/// <summary>Command to create a new notification for a user.</summary>
/// <param name="UserId">Recipient user ID.</param>
/// <param name="Title">Short notification title.</param>
/// <param name="Message">Notification body.</param>
/// <param name="Type">Notification type.</param>
/// <param name="RelatedEntityId">Optional related entity ID.</param>
public sealed record CreateNotificationCommand(
    Guid UserId,
    string Title,
    string Message,
    NotificationType Type,
    Guid? RelatedEntityId = null) : IRequest<Result<Guid>>;
