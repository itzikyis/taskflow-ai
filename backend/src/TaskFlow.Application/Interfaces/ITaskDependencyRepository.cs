using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="TaskDependency"/> aggregates.</summary>
public interface ITaskDependencyRepository
{
    /// <summary>Returns a dependency by id, or null.</summary>
    Task<TaskDependency?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns every dependency in the system (used for the timeline and cycle checks).</summary>
    Task<IReadOnlyList<TaskDependency>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns all dependencies that involve the given task (either side).</summary>
    Task<IReadOnlyList<TaskDependency>> GetByTaskAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>Returns true if the exact dependency already exists.</summary>
    Task<bool> ExistsAsync(Guid taskId, Guid blockedByTaskId, CancellationToken ct = default);

    /// <summary>Adds a new dependency to the context.</summary>
    Task AddAsync(TaskDependency dependency, CancellationToken ct = default);

    /// <summary>Marks a dependency for deletion.</summary>
    void Remove(TaskDependency dependency);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
