namespace TaskFlow.Application.Notifications.Dtos;

/// <summary>Data transfer object representing a notification.</summary>
/// <param name="Id">Unique notification identifier.</param>
/// <param name="UserId">Recipient user identifier.</param>
/// <param name="Title">Short notification title.</param>
/// <param name="Message">Notification body.</param>
/// <param name="Type">Notification type as a string.</param>
/// <param name="IsRead">Whether the notification has been read.</param>
/// <param name="RelatedEntityId">Optional related entity ID.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTime CreatedAt);
