using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

/// <summary>Persistence contract for initiatives.</summary>
public interface IInitiativeRepository
{
    Task<IReadOnlyList<Initiative>> GetAllAsync(CancellationToken ct = default);
    Task<Initiative?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Initiative initiative, CancellationToken ct = default);
    Task DeleteAsync(Initiative initiative, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
