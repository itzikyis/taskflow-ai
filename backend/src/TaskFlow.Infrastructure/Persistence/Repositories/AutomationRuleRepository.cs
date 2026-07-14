using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IAutomationRuleRepository"/>.</summary>
internal sealed class AutomationRuleRepository(ApplicationDbContext db) : IAutomationRuleRepository
{
    public async Task<IReadOnlyList<AutomationRule>> GetByProjectAsync(Guid projectId, CancellationToken ct) =>
        await db.AutomationRules
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<AutomationRule?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.AutomationRules.FindAsync([id], ct);

    public async Task AddAsync(AutomationRule rule, CancellationToken ct) =>
        await db.AutomationRules.AddAsync(rule, ct);

    public Task DeleteAsync(AutomationRule rule, CancellationToken ct)
    {
        db.AutomationRules.Remove(rule);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
