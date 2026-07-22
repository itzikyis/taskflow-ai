using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Aggregate root representing a subscription to an external iCalendar feed for a project.
/// Importing events from the feed creates or updates task due dates within the project.
/// </summary>
public sealed class CalendarSubscription : AggregateRoot
{
    private CalendarSubscription() { } // EF Core constructor

    private CalendarSubscription(Guid id, Guid projectId, string externalUrl, string displayName)
    {
        Id = id;
        ProjectId = projectId;
        ExternalUrl = externalUrl;
        DisplayName = displayName;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the project this subscription belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the external iCalendar feed URL to import events from.</summary>
    public string ExternalUrl { get; private set; } = string.Empty;

    /// <summary>Gets the human-readable display name for this subscription.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the last successful sync, or null if never synced.</summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>Gets the UTC timestamp when this subscription was created.</summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Creates a new <see cref="CalendarSubscription"/>.
    /// Returns a failure result when any required field is invalid.
    /// </summary>
    /// <param name="projectId">The ID of the project to associate with.</param>
    /// <param name="externalUrl">The iCal feed URL to import events from.</param>
    /// <param name="displayName">A human-readable label for this subscription.</param>
    public static Result<CalendarSubscription> Create(Guid projectId, string externalUrl, string displayName)
    {
        if (projectId == Guid.Empty)
            return Result<CalendarSubscription>.Failure(CalendarSubscriptionErrors.ProjectIdRequired);

        if (string.IsNullOrWhiteSpace(externalUrl))
            return Result<CalendarSubscription>.Failure(CalendarSubscriptionErrors.ExternalUrlRequired);

        if (externalUrl.Length > 2000)
            return Result<CalendarSubscription>.Failure(CalendarSubscriptionErrors.ExternalUrlTooLong);

        if (string.IsNullOrWhiteSpace(displayName))
            return Result<CalendarSubscription>.Failure(CalendarSubscriptionErrors.DisplayNameRequired);

        if (displayName.Length > 200)
            return Result<CalendarSubscription>.Failure(CalendarSubscriptionErrors.DisplayNameTooLong);

        return Result<CalendarSubscription>.Success(
            new CalendarSubscription(Guid.NewGuid(), projectId, externalUrl.Trim(), displayName.Trim()));
    }

    /// <summary>Records the current UTC time as the last successful sync timestamp.</summary>
    public void MarkSynced() => LastSyncedAt = DateTime.UtcNow;
}
