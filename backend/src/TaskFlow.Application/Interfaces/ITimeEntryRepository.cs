using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="TimeEntry"/> aggregates.</summary>
public interface ITimeEntryRepository
{
    /// <summary>Returns a time entry by id, or null if not found.</summary>
    Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all time entries for a task, newest first.</summary>
    Task<IReadOnlyList<TimeEntry>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>Returns all time entries for a user within a date range (UTC, inclusive).</summary>
    Task<IReadOnlyList<TimeEntry>> GetByUserAndDateRangeAsync(
        Guid userId, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>Returns the total minutes logged against a task.</summary>
    Task<int> GetTotalMinutesByTaskAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>Adds a new time entry to the context.</summary>
    Task AddAsync(TimeEntry entry, CancellationToken ct = default);

    /// <summary>Marks a time entry for deletion.</summary>
    void Remove(TimeEntry entry);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
