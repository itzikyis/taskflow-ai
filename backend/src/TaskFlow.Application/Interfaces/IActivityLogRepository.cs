using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository contract for the <see cref="ActivityLog"/> aggregate.</summary>
public interface IActivityLogRepository
{
    /// <summary>Gets activity log entries for a specific entity, ordered by most recent first.</summary>
    Task<IReadOnlyList<ActivityLog>> GetByEntityAsync(string entityType, Guid entityId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Gets activity log entries scoped to a project, ordered by most recent first.</summary>
    Task<IReadOnlyList<ActivityLog>> GetByProjectAsync(Guid projectId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Gets activity log entries performed by a specific actor, ordered by most recent first.</summary>
    Task<IReadOnlyList<ActivityLog>> GetByActorAsync(Guid actorId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Gets the most recent activity log entries across the system, ordered by most recent first.</summary>
    Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Adds a new activity log entry to the store.</summary>
    Task AddAsync(ActivityLog log, CancellationToken ct = default);

    /// <summary>Persists all pending changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
