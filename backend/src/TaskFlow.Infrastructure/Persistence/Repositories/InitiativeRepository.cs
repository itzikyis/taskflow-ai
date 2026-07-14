using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IInitiativeRepository"/>.</summary>
internal sealed class InitiativeRepository(ApplicationDbContext db) : IInitiativeRepository
{
    public async Task<IReadOnlyList<Initiative>> GetAllAsync(CancellationToken ct) =>
        await db.Initiatives.AsNoTracking().OrderByDescending(i => i.CreatedAt).ToListAsync(ct);

    public async Task<Initiative?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Initiatives.FindAsync([id], ct);

    public async Task AddAsync(Initiative initiative, CancellationToken ct) =>
        await db.Initiatives.AddAsync(initiative, ct);

    public Task DeleteAsync(Initiative initiative, CancellationToken ct)
    {
        db.Initiatives.Remove(initiative);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
