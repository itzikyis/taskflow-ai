using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITeamRepository"/>.</summary>
internal sealed class TeamRepository(ApplicationDbContext context) : ITeamRepository
{
    /// <inheritdoc/>
    public Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken ct = default) =>
        await context.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(Team team, CancellationToken ct = default) =>
        await context.Teams.AddAsync(team, ct);

    /// <inheritdoc/>
    public void Update(Team team) => context.Teams.Update(team);

    /// <inheritdoc/>
    public void Remove(Team team) => context.Teams.Remove(team);

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
