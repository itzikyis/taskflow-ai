using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository contract for <see cref="Notification"/> persistence.</summary>
public interface INotificationRepository
{
    /// <summary>Returns a notification by its unique ID, or <see langword="null"/> if not found.</summary>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns a paged list of notifications for the given user, ordered by newest first.</summary>
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns the number of unread notifications for the given user.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Adds a new notification to the store.</summary>
    Task AddAsync(Notification notification, CancellationToken ct = default);

    /// <summary>Marks a tracked notification entity as modified.</summary>
    void Update(Notification notification);

    /// <summary>Schedules a set of notifications for deletion.</summary>
    void RemoveRange(IEnumerable<Notification> notifications);

    /// <summary>Persists all pending changes to the database.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
