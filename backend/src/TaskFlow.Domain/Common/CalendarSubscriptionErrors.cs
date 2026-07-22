namespace TaskFlow.Domain.Common;

/// <summary>Domain errors related to calendar subscription operations.</summary>
public static class CalendarSubscriptionErrors
{
    /// <summary>Raised when the external iCal URL is missing or empty.</summary>
    public static readonly Error ExternalUrlRequired =
        new("CalendarSubscription.ExternalUrlRequired", "The external iCal feed URL is required.");

    /// <summary>Raised when the external iCal URL exceeds the maximum allowed length.</summary>
    public static readonly Error ExternalUrlTooLong =
        new("CalendarSubscription.ExternalUrlTooLong", "The external iCal feed URL must not exceed 2000 characters.");

    /// <summary>Raised when the display name is missing or empty.</summary>
    public static readonly Error DisplayNameRequired =
        new("CalendarSubscription.DisplayNameRequired", "The display name is required.");

    /// <summary>Raised when the display name exceeds the maximum allowed length.</summary>
    public static readonly Error DisplayNameTooLong =
        new("CalendarSubscription.DisplayNameTooLong", "The display name must not exceed 200 characters.");

    /// <summary>Raised when the project ID is not provided.</summary>
    public static readonly Error ProjectIdRequired =
        new("CalendarSubscription.ProjectIdRequired", "A valid project ID is required.");

    /// <summary>Raised when a calendar subscription could not be found.</summary>
    public static readonly Error NotFound =
        new("CalendarSubscription.NotFound", "The calendar subscription was not found.");
}
