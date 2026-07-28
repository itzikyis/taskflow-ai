using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IGoalRepository"/>.</summary>
internal sealed class GoalRepository(ApplicationDbContext context) : IGoalRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Goal>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default) =>
        await context.Goals
            .AsNoTracking()
            .Include("_keyResults")
            .Where(g => g.ProjectId == projectId)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Goals
            .Include("_keyResults")
            .FirstOrDefaultAsync(g => g.Id == id, ct);

    /// <inheritdoc/>
    public async Task AddAsync(Goal goal, CancellationToken ct = default) =>
        await context.Goals.AddAsync(goal, ct);

    /// <inheritdoc/>
    public void Update(Goal goal) =>
        context.Goals.Update(goal);

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var goal = await context.Goals.FindAsync([id], ct);
        if (goal is not null)
            context.Goals.Remove(goal);
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
