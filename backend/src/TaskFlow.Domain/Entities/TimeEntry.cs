using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>Aggregate root representing time logged against a task.</summary>
public sealed class TimeEntry : AggregateRoot
{
    private const int MaxMinutes = 24 * 60; // one day per entry

    private TimeEntry() { }

    private TimeEntry(Guid id, Guid taskId, Guid userId, int minutes, string? note, DateTime loggedAt)
    {
        Id = id;
        TaskId = taskId;
        UserId = userId;
        Minutes = minutes;
        Note = note;
        LoggedAt = loggedAt;
    }

    /// <summary>Gets the task the time was logged against.</summary>
    public Guid TaskId { get; private init; }

    /// <summary>Gets the user who logged the time.</summary>
    public Guid UserId { get; private init; }

    /// <summary>Gets the number of minutes logged.</summary>
    public int Minutes { get; private init; }

    /// <summary>Gets an optional note describing the work.</summary>
    public string? Note { get; private init; }

    /// <summary>Gets the UTC timestamp the work was logged for.</summary>
    public DateTime LoggedAt { get; private init; }

    /// <summary>Records a new time entry after validating its inputs.</summary>
    public static Result<TimeEntry> Create(
        Guid taskId, Guid userId, int minutes, string? note = null, DateTime? loggedAt = null)
    {
        if (minutes <= 0)
            return Result<TimeEntry>.Failure(TimeEntryErrors.MinutesMustBePositive);
        if (minutes > MaxMinutes)
            return Result<TimeEntry>.Failure(TimeEntryErrors.MinutesTooLarge);
        if (note is { Length: > 500 })
            return Result<TimeEntry>.Failure(TimeEntryErrors.NoteTooLong);

        return Result<TimeEntry>.Success(
            new TimeEntry(Guid.NewGuid(), taskId, userId, minutes, note?.Trim(), loggedAt ?? DateTime.UtcNow));
    }
}
