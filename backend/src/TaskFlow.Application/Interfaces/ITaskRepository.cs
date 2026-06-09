using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository contract for the <see cref="TaskItem"/> aggregate root.</summary>
public interface ITaskRepository
{
    /// <summary>Gets a task by its identifier. Returns null if not found.</summary>
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets all tasks, optionally filtered by assigned user.</summary>
    Task<IReadOnlyList<TaskItem>> GetAllAsync(Guid? assignedToUserId = null, CancellationToken cancellationToken = default);

    /// <summary>Adds a new task to the store.</summary>
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>Marks a task as modified.</summary>
    void Update(TaskItem task);

    /// <summary>Removes a task from the store.</summary>
    void Remove(TaskItem task);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
