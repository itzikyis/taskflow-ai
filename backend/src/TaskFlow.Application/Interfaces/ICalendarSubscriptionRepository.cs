using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository contract for the <see cref="CalendarSubscription"/> aggregate root.</summary>
public interface ICalendarSubscriptionRepository
{
    /// <summary>Returns all calendar subscriptions for the specified project.</summary>
    Task<List<CalendarSubscription>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Returns a single subscription by its identifier, or null if not found.</summary>
    Task<CalendarSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Adds a new calendar subscription to the store.</summary>
    Task AddAsync(CalendarSubscription sub, CancellationToken ct = default);

    /// <summary>Removes a calendar subscription from the store.</summary>
    void Remove(CalendarSubscription sub);

    /// <summary>Persists all pending changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
