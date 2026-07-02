using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Repository interface for append-only audit trail entries.
/// Implementations must never expose update or delete operations.
/// </summary>
public interface IAuditRepository
{
    /// <summary>Appends a new <see cref="AuditEntry"/> to the store.</summary>
    /// <param name="entry">The entry to append.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(AuditEntry entry, CancellationToken ct = default);

    /// <summary>Persists all pending changes to the underlying store.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Returns all audit entries for a specific entity, ordered newest-first.</summary>
    /// <param name="entityType">The entity type name (e.g. "Task").</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<AuditEntry>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default);

    /// <summary>Returns a paged list of audit entries created by a specific actor, ordered newest-first.</summary>
    /// <param name="actorId">The actor's user identifier.</param>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Number of entries per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<AuditEntry>> GetByActorAsync(
        Guid actorId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Returns the most recent audit entries across all entities, paged and ordered newest-first.</summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Number of entries per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<AuditEntry>> GetRecentAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);
}
