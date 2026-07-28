using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository contract for the <see cref="Goal"/> aggregate root.</summary>
public interface IGoalRepository
{
    /// <summary>Gets all goals belonging to the specified project, including their key results.</summary>
    Task<IReadOnlyList<Goal>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Gets a goal by its unique identifier, including its key results. Returns null if not found.</summary>
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Adds a new goal to the store.</summary>
    Task AddAsync(Goal goal, CancellationToken ct = default);

    /// <summary>Marks a goal as modified.</summary>
    void Update(Goal goal);

    /// <summary>Deletes a goal by its unique identifier.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
