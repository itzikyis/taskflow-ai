namespace TaskFlow.Application.CalendarSync.Dtos;

/// <summary>Read model for a calendar subscription.</summary>
/// <param name="Id">Unique identifier of the subscription.</param>
/// <param name="ProjectId">ID of the owning project.</param>
/// <param name="ExternalUrl">The iCal feed URL being imported.</param>
/// <param name="DisplayName">Human-readable label for this subscription.</param>
/// <param name="LastSyncedAt">UTC time of the last successful sync, or null if never synced.</param>
public sealed record CalendarSubscriptionDto(
    Guid Id,
    Guid ProjectId,
    string ExternalUrl,
    string DisplayName,
    DateTime? LastSyncedAt);
