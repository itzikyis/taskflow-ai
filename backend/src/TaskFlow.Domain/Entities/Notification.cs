using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

/// <summary>Represents a notification sent to a user.</summary>
public sealed class Notification
{
    /// <summary>Gets the unique identifier of this notification.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the ID of the recipient user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the short title of the notification.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Gets the notification body message.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Gets the notification type.</summary>
    public NotificationType Type { get; private set; }

    /// <summary>Gets a value indicating whether this notification has been read.</summary>
    public bool IsRead { get; private set; }

    /// <summary>Gets the optional ID of the related entity (e.g. a task).</summary>
    public Guid? RelatedEntityId { get; private set; }

    /// <summary>Gets the UTC timestamp when the notification was created.</summary>
    public DateTime CreatedAt { get; private set; }

    private Notification() { }

    /// <summary>Creates a new <see cref="Notification"/>.</summary>
    /// <param name="userId">The recipient user ID.</param>
    /// <param name="title">Short notification title.</param>
    /// <param name="message">Notification body.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="relatedEntityId">Optional related entity ID.</param>
    /// <returns>A new <see cref="Notification"/> instance.</returns>
    public static Notification Create(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedEntityId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>Marks this notification as read.</summary>
    public void MarkAsRead() => IsRead = true;
}
