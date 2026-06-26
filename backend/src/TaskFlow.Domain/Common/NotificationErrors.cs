namespace TaskFlow.Domain.Common;

/// <summary>Domain errors specific to the Notification aggregate.</summary>
public static class NotificationErrors
{
    /// <summary>The requested notification does not exist.</summary>
    public static readonly Error NotFound =
        new("Notification.NotFound", "Notification not found.");

    /// <summary>The requester does not own the notification.</summary>
    public static readonly Error NotOwner =
        new("Notification.NotOwner", "You do not own this notification.");
}
