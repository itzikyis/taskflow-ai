using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Repository interface for the <see cref="Team"/> aggregate.</summary>
public interface ITeamRepository
{
    /// <summary>Returns the team with the given ID, including members, or null if not found.</summary>
    Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all teams including their members.</summary>
    Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Adds a new team to the store.</summary>
    Task AddAsync(Team team, CancellationToken ct = default);

    /// <summary>Marks the team as modified.</summary>
    void Update(Team team);

    /// <summary>Removes the team from the store.</summary>
    void Remove(Team team);

    /// <summary>Persists pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
