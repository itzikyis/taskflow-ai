using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for the <see cref="TaskTemplate"/> aggregate.</summary>
public interface ITaskTemplateRepository
{
    /// <summary>Returns all templates that belong to a given project.</summary>
    Task<IReadOnlyList<TaskTemplate>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>Returns a template by its unique identifier, or null if not found.</summary>
    Task<TaskTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new template.</summary>
    Task AddAsync(TaskTemplate template, CancellationToken cancellationToken = default);

    /// <summary>Removes a template from the store.</summary>
    void Delete(TaskTemplate template);

    /// <summary>Saves all pending changes to the underlying store.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
