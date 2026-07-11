using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="TaskDevelopmentLink"/> aggregates.</summary>
public interface IDevelopmentLinkRepository
{
    /// <summary>Returns a development link by id, or null if not found.</summary>
    Task<TaskDevelopmentLink?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all development links for a given task, newest first.</summary>
    Task<IReadOnlyList<TaskDevelopmentLink>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>
    /// Returns an existing link for the same task, repository and external id, or
    /// null. Used to update (rather than duplicate) a reference when a webhook
    /// fires repeatedly for the same commit/PR.
    /// </summary>
    Task<TaskDevelopmentLink?> FindByExternalRefAsync(
        Guid taskId, string repository, string externalId, CancellationToken ct = default);

    /// <summary>Adds a new development link to the context.</summary>
    Task AddAsync(TaskDevelopmentLink link, CancellationToken ct = default);

    /// <summary>Marks a development link for deletion.</summary>
    void Remove(TaskDevelopmentLink link);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
