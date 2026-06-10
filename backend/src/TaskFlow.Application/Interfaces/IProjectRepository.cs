using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository contract for the <see cref="Project"/> aggregate root.</summary>
public interface IProjectRepository
{
    /// <summary>Gets a project by its identifier. Returns null if not found.</summary>
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets all projects owned by the specified user.</summary>
    Task<IReadOnlyList<Project>> GetAllAsync(Guid? ownerId = null, CancellationToken cancellationToken = default);

    /// <summary>Adds a new project.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Marks a project as modified.</summary>
    void Update(Project project);

    /// <summary>Removes a project.</summary>
    void Remove(Project project);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
