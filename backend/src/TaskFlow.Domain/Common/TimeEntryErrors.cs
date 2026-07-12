namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for time tracking.</summary>
public static class TimeEntryErrors
{
    /// <summary>Returned when the logged minutes are zero or negative.</summary>
    public static readonly Error MinutesMustBePositive =
        new("TimeEntry.MinutesMustBePositive", "Logged minutes must be greater than zero.");

    /// <summary>Returned when a single entry exceeds 24 hours.</summary>
    public static readonly Error MinutesTooLarge =
        new("TimeEntry.MinutesTooLarge", "A single time entry cannot exceed 24 hours (1440 minutes).");

    /// <summary>Returned when the note is too long.</summary>
    public static readonly Error NoteTooLong =
        new("TimeEntry.NoteTooLong", "Note must not exceed 500 characters.");

    /// <summary>Returned when the requested time entry does not exist.</summary>
    public static readonly Error NotFound =
        new("TimeEntry.NotFound", "Time entry was not found.");
}
