using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for <see cref="Board"/> aggregates.</summary>
public interface IBoardRepository
{
    /// <summary>Returns a board by id including its columns, or null if not found.</summary>
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all boards for the given project, ordered by creation date descending.</summary>
    Task<IReadOnlyList<Board>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>Adds a new board to the context.</summary>
    Task AddAsync(Board board, CancellationToken ct = default);

    /// <summary>Marks a board as modified.</summary>
    void Update(Board board);

    /// <summary>Marks a board for deletion.</summary>
    void Remove(Board board);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
