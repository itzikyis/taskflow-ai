using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="Comment"/> aggregates.</summary>
public interface ICommentRepository
{
    /// <summary>Returns a comment by its identifier, or <c>null</c> if not found.</summary>
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all comments belonging to the specified task.</summary>
    Task<IReadOnlyList<Comment>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);

    /// <summary>Adds a new comment to the persistence store.</summary>
    Task AddAsync(Comment comment, CancellationToken ct = default);

    /// <summary>Marks a comment as modified.</summary>
    void Update(Comment comment);

    /// <summary>Removes a comment from the persistence store.</summary>
    void Remove(Comment comment);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
