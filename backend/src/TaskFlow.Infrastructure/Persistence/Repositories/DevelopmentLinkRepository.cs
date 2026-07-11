using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IDevelopmentLinkRepository"/>.</summary>
internal sealed class DevelopmentLinkRepository(ApplicationDbContext context) : IDevelopmentLinkRepository
{
    public Task<TaskDevelopmentLink?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.DevelopmentLinks.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<TaskDevelopmentLink>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
        await context.DevelopmentLinks
            .Where(l => l.TaskId == taskId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

    public Task<TaskDevelopmentLink?> FindByExternalRefAsync(
        Guid taskId, string repository, string externalId, CancellationToken ct = default) =>
        context.DevelopmentLinks.FirstOrDefaultAsync(
            l => l.TaskId == taskId && l.Repository == repository && l.ExternalId == externalId, ct);

    public async Task AddAsync(TaskDevelopmentLink link, CancellationToken ct = default) =>
        await context.DevelopmentLinks.AddAsync(link, ct);

    public void Remove(TaskDevelopmentLink link) => context.DevelopmentLinks.Remove(link);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
